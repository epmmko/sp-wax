
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* * * * * *                  Coutinho                 * * * * * *
* * * * * *          Solid-liquid Equilibrium         * * * * * *
* * * * * *              Thermodynamic Model          * * * * * *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

//In this .CPP file, SLE characteristics at one temperature are reported

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

//This function returns the current directory
string ExeDir1()
{
	char Var[MAX_PATH];
	GetModuleFileName(NULL, Var, MAX_PATH);
	string::size_type pos = string(Var).find_last_of("\\/");
	return string(Var).substr(0, pos);
}

//Ideal gas constant
const double R = 8.3144621; // J/Mol/K -All units are in SI 


int main()
{
	/********************************Inputs*************************************/

	//Direcotry
	string Directory = ExeDir1();

	//Paraffin weight fraction in the oil sample
	double NalkaneWeightFraction;

	//Correction factor coefficient 
	double CF;

	//Total number of components (n-alkanes)
	int CompNum;

	//Minimum carbon number which is desired to be included in the calculations 
	int MinC;

	//First srtarting temperature 
	double DisiredTemp;

	/*Maximum acceptable Error. This number has been checked by several results
	and is chosen to be an optimum value. If potential user wants better accuracy
	he is adviced to decrease this value
	*/
	double Epsilon = pow(10, -4);

	/***************************End of Inputs********************************/


	//Reading files
	ifstream CompositionNalkanes, PrecipitatationCurveWAT, kk, KInput,
		GeneralInputs, TempCase, NumLine, KInitialVal;
	GeneralInputs.open(Directory + "\\GeneralInputs.txt");
	PrecipitatationCurveWAT.open(Directory + "\\PrecipitatationCurveWAT.txt");
	CompositionNalkanes.open(Directory + "\\Data.txt");
	KInput.open(Directory + "\\KInput.txt");
	TempCase.open(Directory + "\\TempCase.txt");
	NumLine.open(Directory + "\\NumLine.txt");
	KInitialVal.open(Directory + "\\KInitialVal.txt");


	//Output files
	ofstream nSFile, temperature, WATT, OutSolidW, OutSolidMolar, OutLiquidMolar,
		OutLiquidW, k, Concentration, Gradient, OUTPUT, DisMass;
	k.open(Directory + "\\K_values.txt");
	OutSolidMolar.open(Directory + "\\SolidMolarComposition.txt");
	OutLiquidMolar.open(Directory + "\\LiquidMolarComposition.txt");
	OutLiquidW.open(Directory + "\\LiquidWComposition.txt");
	Concentration.open(Directory + "\\RelativeConcentration.txt");
	DisMass.open(Directory + "\\DissolvedMass.txt");
	OutSolidW.open(Directory + "\\SolidWCompositionOnetemp.txt");
	nSFile.open(Directory + "\\OneTemp Wax weight fraction.txt");

	//Error of each component
	double sumErr[100 + 1];

	//Input nalkane mole fractions (summation of all mole fractions is equal to 1)
	double Z[100 + 1];

	/*Total weight of n-alkanes if it is assumed that all components are in liquid
	phase and summation of their mole fractions is one*/
	double  SumWOil;

	/*Total weight of solid phase if it is assumed that molar composition of all
	carbon numbers (in solid phase) goes to one*/
	double sumWS = 0;

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
	double nS = 0.000000220462;

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

	//Density of liquid phase 
	double Density[100];

	//carbon number identification or CN
	int CarbonNum[100 + 1];

	//Number of assigned threads
	omp_set_num_threads(NumThreads);

	//Counter
	double Count = 0, Count1 = 0;

	//Input K-values
	double KIn[120][100 + 1];

	//Concentration array
	double Conc[100 + 1];

	//Dissolved mass array
	double DisMas[100 + 1];

	//Volume and mass 
	double VLiq, MLiq;

	//Local usefull variables (no certain meaning)
	double sumM = 0;
	double SumWS = 0;
	double	a;
	double nsGuess;
	int Number;
	int click = 0;


	//calling DIPPR molar volume coefficients
	ArrayRetA();
	ArrayRetB();
	ArrayRetC();
	ArrayRetD();
	//Assigning different values from the text file
	GeneralInputs >> NalkaneWeightFraction;
	NalkaneWeightFraction = NalkaneWeightFraction / 100.0;
	GeneralInputs >> CF;
	GeneralInputs >> MinC;
	GeneralInputs >> CompNum;
	MinC = 10;
	TempCase >> DisiredTemp;
	NumLine >> Number;

	SumWOil = 0;
	for (int i = 1; i < CompNum + 1; i++)
	{
		/*Assigning carbon number identification
		Assigning carbon numbers to the array. CarbonNum[] is used as
		*/
		CarbonNum[i] = i;

		//Reading compositions from the text files and insert them to the assigned arrays
		CompositionNalkanes >> Z[i];

		//Calculating weight of each carbon number component
		WOil[i] = Z[i] * (12 * CarbonNum[i] + 2 * CarbonNum[i] + 2);

		//Total weight calculations
		SumWOil = SumWOil + WOil[i];
	}

	/*This "for loop" is to give initial guess values for equilibrium constant for
	non-participating carbon number components*/
	for (int i = 1; i < MinC; i++) {
		K[i] = 0.0;
		KC[i] = 0.0;
	}

	//K-Values are read from the text file which were generated by SPWaxKInitialization.exe
	for (int j = 0; j < Number; j++) {
		for (int i = 1; i < CompNum + 1; i++) {
			KInitialVal >> KIn[j][i];
		}
	}


	/*Temperature based initial values for equilibrium constants for participating
	n-alkane components*/
	if (DisiredTemp >= 280.15)
	{
		for (int i = MinC; i < CompNum + 1; i++)
		{
			double x = (DisiredTemp - 280.15) * 2;
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

	//For temperatures less than 280.15K, convergence problem is not encountered
	if (DisiredTemp < 280.15)
	{
		for (int i = MinC; i < CompNum + 1; i++)
		{
			double x = DisiredTemp - 280.15;
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

			T = DisiredTemp;
			/*This while loop makes sure nS is optimally chosen and all equilibrium
			constants have been chosen correctly*/
			while (abs(Err) > Epsilon)
			{
				//Based on given K-values, nS is picked through the following function 
				nS = Fsolve(nS, CompNum, Z, K);
				nL = 1 - nS;
				click = 0;
				//Based on the chosen nS, following parameters are calculated
				for (int i = 1; i < CompNum + 1; i++)
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
						sumErr[i] = abs(K[i] - KC[i]) / KC[i];
					}
					else
					{
						sumErr[i] = 0;
					}
				}

				//Total error calculation
				Err = 0;
				//At this "for loop", total Error is calculated 
#pragma omp parallel for reduction (+:Err)
				for (int jk = MinC; jk < CompNum + 1; jk++)
				{
					Err = Err + sumErr[jk] / (CompNum - MinC);
				}
				if (nS > 0)
				{
					nsGuess = nS;
				}
			}

			sumWS = 0;
			for (int i = 1; i < CompNum + 1; i++)
			{ /*This for loop, calculatese the summations of all components of each
			  phase. It basically used to check if both phase's compositinos add up to unity*/

				sumWS = sumWS + XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			}

			//Weight composition of solid phase is calculated
			for (int i = 1; i < CompNum + 1; i++)
			{
				SolidCompW[i] = XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / sumWS;
			}

			Err = 200;
			isnan(nS) ? a = 1 : 0;
			if (a == 1)
			{
				cout << " No precipitation at selected temperature " << endl;
			}

			SumW = 0;
#pragma omp parallel for reduction (+:SumW)
			for (int i = 1; i < CompNum + 1; i++)
			{
				SumW = SumW + nS * XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			}

			Err = 200;

			//At this for loop, when temperature is reached *within 0.1C accuracy", the 
			//Properties are reported
			if (abs(T - DisiredTemp) <= 0.1)
			{
				a = 1;
			}
		
		SumWS = 0;
		SumWL = 0;
		SumW = 0;
		//The summation parameters are used to normalize the compositions
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


		//Weight fraction of each component in liquid and solid phase 
		for (int i = 1; i < CompNum + 1; i++)
		{
			LiquidCompW[i] = (XL[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / SumWL);
			SolidCompW[i] = XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / SumWS;
			k << K[i] << endl;
		}

		//Liquid volume calculation
		SumVol = 0;
		sumM = 0;
		for (int ia = 1; ia < CompNum + 1; ia++)
		{
			SumVol = SumVol + (LiquidCompW[ia] * (1 - SumW / SumWOil)) /
				(0.001*(12 * ia + 2 * ia + 2) / Vm(T, ia));
			sumM = sumM + (NalkaneWeightFraction*(LiquidCompW[ia] *
				(1 - SumW / SumWOil)));
		}

		//Dissolved concentration calculations
		for (int ij = 1; ij < CompNum + 1; ij++)
		{
			VLiq = SumVol;
			MLiq = (NalkaneWeightFraction*(LiquidCompW[ij] * (1 - SumW / SumWOil)));
			Conc[ij] = MLiq / VLiq;
			DisMas[ij] = MLiq;
		}

		Err = 200;


	  //Output reporting to text files
	nSFile << setprecision(8) << fixed << NalkaneWeightFraction * SumW / SumWOil << endl;
	a = NalkaneWeightFraction * SumW / SumWOil;
	for (int i = 1; i < CompNum + 1; i++)
	{

		if (i < 10)
		{
			OutLiquidMolar << "C" << i << "	" << setprecision(9) << fixed << XL[i] << endl;
			OutSolidMolar << "C" << i << "	" << setprecision(9) << fixed << XS[i] << endl;
			OutLiquidW << "C" << i << "	" << setprecision(9) << fixed << LiquidCompW[i] << endl;
			OutSolidW << "C" << i << "	" << setprecision(9) << fixed << SolidCompW[i] << endl;
			Concentration << "C" << i << "	" << setprecision(9) << fixed << Conc[i] << endl;
			DisMass << "C" << i << "	" << setprecision(9) << fixed << DisMas[i] << endl;
		}
		else
		{
			OutLiquidMolar << "C" << i << "	" << setprecision(9) << fixed << XL[i] << endl;
			OutSolidMolar << "C" << i << "	" << setprecision(9) << fixed << XS[i] << endl;
			OutLiquidW << "C" << i << "	" << setprecision(9) << fixed << LiquidCompW[i] << endl;
			OutSolidW << "C" << i << "	" << setprecision(9) << fixed << SolidCompW[i] << endl;
			Concentration << "C" << i << "	" << setprecision(9) << fixed << Conc[i] << endl;
			DisMass << "C" << i << "	" << setprecision(9) << fixed << DisMas[i] << endl;
		}

	}

	nSFile.close();
	WATT.close();
	temperature.close();
	OutSolidW.close();
	temperature.close();
	WATT.close();
	OutSolidW.close();



}

