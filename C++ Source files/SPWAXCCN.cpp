
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* * * * * *                  Coutinho                 * * * * * *
* * * * * *          Solid-liquid Equilibrium         * * * * * *
* * * * * *              Thermodynamic Model          * * * * * *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*In this .CPP file, relative concentration gradient is calculated
which can be used to determine CCN and to verify aging process*/

//Used libraries
#include <iostream>
#include <cmath>
#include <fstream>
#include <time.h>
#include "Header.h"
#include <iomanip> 
#include "omp.h"
#include <windows.h>
#include <string>
using namespace std;

//Number of threads
const int NumThreads = 8;

//Ideal gas constant
const double R = 8.3144621; // J/Mol/K -All units are in SI 

// Function that returns current direction
string ExeDir1()
{
	char Var[MAX_PATH];
	GetModuleFileName(NULL, Var, MAX_PATH);
	string::size_type pos = string(Var).find_last_of("\\/");
	return string(Var).substr(0, pos);
}

int main()
{
	/********************************Inputs*************************************/

	//Current Directory
	string Directory;
	Directory = ExeDir1();

	//Paraffin weight fraction in the oil sample
	double NalkaneWeightFraction;

	//Correction factor coefficient 
	double CF;

	//Total number of components (n-alkanes)
	int CompNum;

	//Minimum carbon number which is desired to be included in the calculations 
	int MinC;

	//First srtarting temperature 
	double DesiredTemp;

	/*Maximum acceptable Error. User can change this value as needed to increase or
	decrease the accuracy*/
	double Epsilon = pow(10, -4);

	/***************************End of Inputs********************************/

	//Reading files
	ifstream CompositionNalkanes, PrecipitatationCurveWAT, kk, KInput, NumLine, KInitialVal,
		GeneralInputs, TempCaseCCN;
	GeneralInputs.open(Directory + "\\GeneralInputs.txt");
	PrecipitatationCurveWAT.open(Directory + "\\PrecipitatationCurveWAT.txt");
	kk.open(Directory + "\\kk.txt");
	CompositionNalkanes.open(Directory + "\\Data.txt");
	KInput.open(Directory + "\\KInput.txt");
	TempCaseCCN.open(Directory + "\\TempCaseCCN.txt");
	NumLine.open(Directory + "\\NumLine.txt");
	KInitialVal.open(Directory + "\\KInitialVal.txt");


	//Output files
	ofstream Gradient;
	Gradient.open(Directory + "\\RelativeConcentrationGradient.txt");

	//Error of each component
	double SumErr[100 + 1];

	//Input n-alkane mole fractions (summation of all mole fractions is equal to 1)
	double Z[100 + 1];

	/*Total weight of n-alkanes if it is assumed that all components are in liquid 
	phase and summation of their mole fractions is one*/
	double  SumWOil;

	/*Total weight of solid phase if it is assumed that molar composition of all 
	carbon numbers (in solid phase) goes to one*/
	double  SumWS;

	/*Total weight of liquid phase if it is assumed that molar composition of all 
	carbon numbers (in liquid phase) goes to one*/
	double  SumWL;

	/*Total weight of solid phase in n-alkane system if it is assumed that total 
	n-alkanes equal to one mole */
	double	SumW;

	//Volume of liquid at one temperature
	double SumVol;

	//Normalized solid phase molar composition
	double	XS[100 + 1];

	//Normalized Liquid phase molar composition
	double	XL[100 + 1];

	//Equilibrium constant values for a temperature (New)
	double 	K[100 + 1];

	//Equilibrium constant values for a temperature (old)
	double	 KC[100 + 1];

	/*weight of each carbon number component if all n-alkanes are in liquid phase
	and summation of all carbon number mole fractions goes to one*/
	double 	WOil[100 + 1];

	// Error calculation between of old and new K_values 
	double Err = 1000;

	/*Mole fraction of n-alkanes in liquid phase at a certain temperature if it is
	assumed that summation of all n-alkanes (liquid+solid) is one mole*/
	double nL;

	//Temperature
	double T;

	/*Mole fraction of n-alkanes in liquid phase at a certain temperature if it is
	assumed that summation of all n-alkanes (liquid+solid) is one mole*/
	double nS = 0.0000220462;

	//Molecular weight (Local variable)
	double Mw;

	/*Normalized mole fraction of all components in liquid phase for all selected 
	temperatures*/
	double LiquidComp[100 + 1];

	/*Normalized mole fraction of all components in solid phase for all selected 
	temperatures*/
	double SolidComp[100 + 1];

	/*Normalized wight fraction of all components in liquid phase for all selected
	temperatures*/
	double LiquidCompW[100 + 1];

	/*Normalized wight fraction of all components in solid phase for all selected
	temperatures*/
	double SolidCompW[100 + 1];

	//carbon number identification or CN
	int CarbonNum[100 + 1];

	//Number of assigned threads
	omp_set_num_threads(NumThreads);

	//Input K-values (will be read from text file)
	double KIn[120][100 + 1];

	//Concentration array
	double Conc[2][100 + 1];

	//concentration gradient between wall and interface
	double gradient[100 + 1];

	double V_Liq, M_Liq;

	//Local usefull variables (no certain meaning
	double sumM = 0;
	double	a;
	double sumWS = 0;
	double nsGuess;
	int Number;
	//new
	double NewTemp;
	double TempCCN[2];


	//calling DIPPR molar volume coefficients
	ArrayRetA();
	ArrayRetB();
	ArrayRetC();
	ArrayRetD();

	// Variables are assigned from text files
	GeneralInputs >> NalkaneWeightFraction;
	NalkaneWeightFraction = NalkaneWeightFraction / 100.0;
	GeneralInputs >> CF;
	GeneralInputs >> MinC;
	GeneralInputs >> CompNum;
	MinC = 10;
	TempCaseCCN >> TempCCN[0];
	TempCaseCCN >> TempCCN[1];
	NumLine >> Number;
	SumWOil = 0;
	
	for (int i = 1; i < CompNum + 1; i++)
	{
		//Assigning carbon number identification
		CarbonNum[i] = i; /* Assigning carbon numbers to the array. CarbonNum[]
		is used as an input in many functions*/

		//Reading compositions from text files and insert them to the assigned arrays

		CompositionNalkanes >> Z[i];

		//Calculating weight of each carbon number component
		WOil[i] = Z[i] * (12 * CarbonNum[i] + 2 * CarbonNum[i] + 2);

		//Total weight calculations
		SumWOil = SumWOil + WOil[i];
	}

	//the input data from the created from C# are inserted in the program

	/*This "for loop" is to give initial guess values for equilibrium constant for
	non-participating carbon number components*/
	for (int i = 1; i < MinC; i++) 
	{
		K[i] = 0.0;
		kk >> a;
		KC[i] = 0.0;
	}

	//K-values for T= 280.15K to 210.15K
	for (int j = 0; j < Number; j++) {
		for (int i = 1; i < CompNum + 1; i++) {
			KInitialVal >> KIn[j][i];

		}
	}

	//In this "for loop", higher and lower temperatures will be considered
	for (int iC = 0; iC < 2; iC++)
	{
		DesiredTemp = TempCCN[iC];
		/*Temperature based initial values for equilibrium constants for participating
		n-alkane components*/
		if (DesiredTemp >= 280.15)
		{
			for (int i = MinC; i < CompNum + 1; i++)
			{
				double x = (DesiredTemp - 280.15) * 2;
				if (abs(KIn[(int)x][i])> 1.0*pow(10, -15))
				{
					K[i] = KIn[(int)x][i];
					a = K[i];
				}

				else
				{
					K[i] = a;
				}

			}
		}
		if (DesiredTemp < 280.15)
		{
			for (int i = MinC; i < CompNum + 1; i++)
			{
				double x = DesiredTemp - 280.15;
				if (abs(KIn[0][i])> 1.0*pow(10, -15))
				{
					K[i] = KIn[0][i];
					a = K[i];
				}

				else
				{
					K[i] = a;
				}

			}
		}

		T = DesiredTemp;
				/*This while loop makes sure nS is optimally chosen and all equilibrium
				constants have been chosen correctly*/   
				while (abs(Err) > Epsilon)
				{
					//Based on given K-values, nS is picked through the following function 
					nS = Fsolve(nS, CompNum, Z, K);
					nL = 1 - nS;
					//Based on the chosen nS, following parameters are calculated
					for (int i = 1; i < CompNum + 1; i++)
					{
						{
							//This if-statement takes excludese components with zero composition
							if (Z[i] == 0)
							{
								XL[i] = 0;
								XS[i] = 0;
							}
							else
							{
								/*Based on the newly calculated nS, it is possible to calculate the liquid
								and solid composition of n-Alkanes*/
								XL[i] = Z[i] / (1 + nS * (K[i] - 1));
								XS[i] = Z[i] * K[i] / (1 + nS * (K[i] - 1));
								KC[i] = K[i];
							}

						}
					}

					//New equilibrium constants are calculated for the selected temperature
#pragma omp parallel for
					for (int i = MinC; i < CompNum + 1; i++)
					{
						if (Z[i] != 0)
						{
							K[i] = (GammaLiqMesEFV(MinC, i, CarbonNum[i], CompNum, CarbonNum, XL, T) /
								GammaSolMes(MinC, CarbonNum[i], CompNum, CarbonNum, XS, T, CF))
								*exp(((1000 * HeatFus(CarbonNum[i]) / (R*TempFus(CarbonNum[i])))
									*(TempFus(CarbonNum[i]) / T - 1)) + ((1000 * HeatTrans(CarbonNum[i])
										/ (R*TempTrans(CarbonNum[i])))*(TempTrans(CarbonNum[i]) / T - 1)) -
										(((4.1868*(0.3033*((12 * CarbonNum[i] + CarbonNum[i] * 2 + 2)) -
											4.635*pow(10, -4)*T*((12 * CarbonNum[i] + CarbonNum[i] * 2 + 2)))) / R)
											*(TempFus(CarbonNum[i]) / T - log(TempFus(CarbonNum[i]) / T) - 1)));

							/*Difference between old and new equilibrium constants are calculated and
							saved for each carbon number*/
							SumErr[i] = abs(K[i] - KC[i]) / KC[i];
						}
						else
						{
							SumErr[i] = 0;
						}
					}

					//Total error calculation
					Err = 0;
#pragma omp parallel for reduction (+:Err)
					for (int jk = MinC; jk < CompNum + 1; jk++) 
					{
						Err = Err + SumErr[jk] / (CompNum - MinC);
					}
					if (nS > 0) 
					{
						nsGuess = nS;
					}
				}

				sumWS = 0;
				for (int i = 1; i < CompNum + 1; i++) 
				{ 
				/*This for loop, calculatese the summations of all components of each phase.
				It basically used to check if both phase's compositinos add up to unity*/

					sumWS = sumWS + XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
				}

				for (int i = 1; i < CompNum + 1; i++) 
				{
					SolidCompW[i] = XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / 
						sumWS;
				}

				Err = 200;
				isnan(nS) ? a = 1 : 0;
				if (a == 1) 
				{
					cout << " No precipitation at selected temperature " << endl;
					break;
				}

				SumW = 0;
#pragma omp parallel for reduction (+:SumW)
				for (int i = 1; i < CompNum + 1; i++) 
				{
					SumW = SumW + nS * XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
				}

				Err = 200;

				//Calculates the time of SLE calculations in a temperature 
				if (abs(T - DesiredTemp) <= 0.1)
				{
					a = 1;
				}
			
			SumWS = 0;
			SumWL = 0;
			SumW = 0;

#pragma omp parallel for reduction (+:SumWS)
			for (int i = 1; i < CompNum + 1; i++) 
			{
				SumWS = SumWS + XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			}

#pragma omp parallel for reduction (+:SumWL)
			for (int i = 1; i < CompNum + 1; i++) 
			{
				SumWL = SumWL + XL[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			}

#pragma omp parallel for reduction (+:SumW)
			for (int i = 1; i < CompNum + 1; i++) 
			{
				SumW = SumW + nS * XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			}

			/*Second column of nSFile.txt is the weight fraction of solid phase in the total
			oil sample (n-alkanes + non n-alkanes) */
			//nSFile << setprecision(15) << NalkaneWeightFraction * SumW / SumWOil << endl;

			//Weight fraction of each component in liquid and solid phase 
			for (int i = 1; i < CompNum + 1; i++) 
			{
				LiquidCompW[i] = (XL[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / 
					SumWL);
				SolidCompW[i] = XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / 
					SumWS;
			}
			//k << endl;

			//Liquid volume calculation
			SumVol = 0;
			sumM = 0;

			for (int ia = 1; ia < CompNum + 1; ia++)
			{
				SumVol = SumVol + (LiquidCompW[ia] * (1 - SumW / SumWOil)) / (0.001*(12 
					* ia + 2 * ia + 2) / Vm(T, ia));
				sumM = sumM + (NalkaneWeightFraction*(LiquidCompW[ia] * (1 - SumW / SumWOil)));
			}

			//Dissolved concentration calculations
			for (int ij = 1; ij < CompNum + 1; ij++)
			{
				V_Liq = SumVol;
				M_Liq = (NalkaneWeightFraction*(LiquidCompW[ij] * (1 - SumW / SumWOil)));
				Conc[iC][ij] = M_Liq / V_Liq;
			}

			Err = 200;
	} 

	// In this "for loop", local concentration gradient is calculated
	for (int i = 1; i < CompNum + 1; i++) 
	{
		gradient[i] = Conc[0][i] - Conc[1][i];
	}
	
	//Output reporting to text file 
	for (int i = 1; i < CompNum + 1; i++) 
	{
		if (i < 10) 
		{
			Gradient << "C" << i << "	" << setprecision(9) << fixed << gradient[i] << endl;
		}
		else 
		{
			Gradient << "C" << i << "	" << setprecision(9) << fixed << gradient[i] << endl;
		}
	}
	Gradient.close();
}
