
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* * * * * *                  Coutinho                 * * * * * *
* * * * * *          Solid-liquid Equilibrium         * * * * * *
* * * * * *              Thermodynamic Model          * * * * * *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

//In this cpp file, precipitation curve and WAT are calculated and reported

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

//Function to get current directory
string ExeDir() {
	char Var[MAX_PATH];
	GetModuleFileName(NULL, Var, MAX_PATH);
	string::size_type pos = string(Var).find_last_of("\\/");
	return string(Var).substr(0, pos);
}

//Global Variables
//Number of threads
const int NumThreads = 8;

//Ideal gas constant
const double R = 8.3144621; // J/Mol/K -All units are in SI 

int main()
{
	/********************************Inputs*************************************/
	string Directory = ExeDir();
	//Paraffin weight fraction in the oil sample
	double NalkaneWeightFraction = 0.147739;

	//Correction factor coefficient 
	double CF;

	//Total number of components (n-alkanes)
	int CompNum;

	//Minimum carbon number which is desired to be included in the calculations 
	int MinC;

	//Number of temperatures
	int NumTemp;

	//First srtarting temperature 
	double StartTemp;

	//Maximum acceptable Error
	double Epsilon;

	//Temperature step
	double TempInterval;

	//Accuracy of WAT
	double WATaccuracy;


	/***************************End of Inputs********************************/

	//Reading files
	ifstream CompositionNalkanes, PrecipitatationCurveWAT, NumLine, KInitialVal,
		GeneralInputs;
	GeneralInputs.open(Directory + "\\GeneralInputs.txt");
	NumLine.open(Directory + "\\NumLine.txt");
	PrecipitatationCurveWAT.open(Directory + "\\PrecipitatationCurveWAT.txt");
	CompositionNalkanes.open(Directory + "\\Data.txt");
	KInitialVal.open(Directory + "\\KInitialVal.txt");

	//Output files
	ofstream nSFile, Temperature, WATT, OutSolidW;
	OutSolidW.open(Directory + "\\SolidWComposition.txt");
	Temperature.open(Directory + "\\temperature.txt");
	nSFile.open(Directory + "\\Wax weight fraction.txt");
	WATT.open(Directory + "\\WAT.txt");

	//Number of threads to be used
	int nthreads;

	//Array to store the error for each component
	double sumErr[100 + 1];

	/*Normalized composition means summation of all carbon numbers should
	be equal to one*/
	clock_t t;

	//Input n-alkane mole fractions (summation of all mole fractions is equal to 1)
	double Z[100 + 1];

	/*Total weight of n-alkanes if it is assumed that all components are
	in liquid phase and summation of their mole fractions is one */
	double  SumWOil;

	/*Total weight of solid phase in n-alkane system if it is assumed
	that total n-alkanes equal to one mole */
	double	SumW;

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
	double nS = 0.0000220462; //Initial Guess

							  //Molecular weight (Local variable)
	double Mw;

	//Selected temperatures
	double Temp[200];

	//Density of liquid phase 
	double Density[100];

	//carbon number identification or CN
	int CarbonNum[100 + 1];

	//2D array to save Solid composition of all temperatures 
	double SolidCompW[200][100 + 1];

	//Number of assigned threads
	omp_set_num_threads(NumThreads);

	//Counter
	double Count = 0, Count1 = 0;

	//Input K-values
	double KIn[120][100 + 1];

	//Local usefull variables (no certain meaning)
	double	a;
	double WATTInput;
	double SumWS = 0;
	double nSGuess;
	int Click = 0;
	double NewTemp;
	int Number;
	//calling DIPPR molar volume coefficients
	ArrayRetA();
	ArrayRetB();
	ArrayRetC();
	ArrayRetD();

	//Assign variables from the text files
	GeneralInputs >> NalkaneWeightFraction;
	NalkaneWeightFraction = NalkaneWeightFraction / 100.0;
	GeneralInputs >> CF;
	GeneralInputs >> MinC;
	GeneralInputs >> CompNum;
	PrecipitatationCurveWAT >> NumTemp;
	PrecipitatationCurveWAT >> StartTemp;
	PrecipitatationCurveWAT >> TempInterval;
	PrecipitatationCurveWAT >> Epsilon;
	PrecipitatationCurveWAT >> WATaccuracy;
	NumLine >> Number;
	SumWOil = 0;

	//In this for loop, different variables are assgined
	for (int i = 1; i < CompNum + 1; i++)
	{
		//Assigning carbon number identification
		/* Assigning carbon numbers to the array. CarbonNum[] is used as an input
		in many functions*/
		CarbonNum[i] = i;

		//Reading compositions from the text files & put them to the assigned arrays
		CompositionNalkanes >> Z[i];

		//Calculating weight of each carbon number component
		WOil[i] = Z[i] * (12 * CarbonNum[i] + 2 * CarbonNum[i] + 2);

		//Total weight calculations
		SumWOil = SumWOil + WOil[i];
	}

	/*This "for loop" is to give initial guess values for equilibrium constant
	for non-participating carbon number components*/
	for (int i = 1; i < MinC; i++) {
		K[i] = 0.0;
		KC[i] = 0.0;
	}

	//K-values for T= 280.15K to 310.15K
	for (int j = 0; j < Number; j++) {
		for (int i = 1; i < CompNum + 1; i++) {
			KInitialVal >> KIn[j][i];

		}
	}

	/*Temperature based initial values for equilibrium constants for participating
	n-alkane components*/
	if (StartTemp >= 280.15)
	{
		for (int i = MinC; i < CompNum + 1; i++)
		{
			double x = (StartTemp - 280.15)*2;
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
	if (StartTemp < 280.15)
	{
		for (int i = MinC; i < CompNum + 1; i++)
		{
			double x = StartTemp - 280.15;
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

	//Based on the input data (from user), temperatures will be assigned
	for (int i = 0; i < NumTemp; i++)
	{
		Temp[i] = StartTemp + TempInterval * i;
	}

	//The parameters which will be displayed at time of simulation run
	cout << "Temperature" << setw(4) << ' ' << "execution time" << setw(3)
		<< ' ' << "wax weight fraction" << endl;

	/*This "for loop" iterates the temperature while all calculated properties
	will be saved*/
	for (int j = 0; j < NumTemp; j++)
	{
		T = Temp[j];
		t = clock();

		/*This while loop makes sure nS is optimally chosen and all equilibrium
		constants have been chosen correctly*/
		while (abs(Err) > Epsilon)
		{
			//Based on given K-values, nS is picked through the following function 
			nS = Fsolve(nS, CompNum, Z, K);
			nL = 1 - nS;
			Click = 0;
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
					/*Based on the newly calculated nS, it is possible to calculate the
					liquid and solid composition of n-Alkanes*/
					XL[i] = Z[i] / (1 + nS * (K[i] - 1));
					XS[i] = Z[i] * K[i] / (1 + nS * (K[i] - 1));
					if (XS[i] >(pow(10, -6)) && Click == 0) {
						Click = 1;
						MinC = i;
					}
					/*It saves the previous K values, so they can be compared with the new
					ones which will be estimated in the following section.*/
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
							*(TempFus(CarbonNum[i]) / T - 1)) + ((1000 * HeatTrans(CarbonNum[i]) /
							(R*TempTrans(CarbonNum[i])))*(TempTrans(CarbonNum[i]) / T - 1)) -
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
#pragma omp parallel for reduction (+:Err)
			for (int j = MinC; j < CompNum + 1; j++)
			{
				Err = Err + sumErr[j] / (CompNum - MinC);
			}
			if (nS > 0) {
				nSGuess = nS;
			}
		}

		SumWS = 0;
		for (int i = 1; i < CompNum + 1; i++)
		{ /*This for loop, calculates the summations of all components of each phase.
		  It basically is used to check if both phase's compositinos add up to unity*/

			SumWS = SumWS + XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);


		}
		for (int i = 1; i < CompNum + 1; i++)
		{
			SolidCompW[j][i] = XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2) / SumWS;
		}
		Err = 200;

		//To check if the right temperature is selected
		isnan(nS) ? a = 1 : 0;
		if (a == 1)
		{
			cout << " No precipitation at selected temperature " << endl;
			break;
		}

		//At this if-loop, WAT is calculated
		if (nS < 0 || nS>1) {
			NewTemp = T - TempInterval;

			for (int jj = 0; jj < 500; jj++)
			{
				T = NewTemp + WATaccuracy * jj;
				t = clock();
				while (abs(Err) > Epsilon)
				{
					//It performs NR to solve for the right nS (precipitated more fraction)
					nS = Fsolve(nS, CompNum, Z, K);
					nL = 1 - nS;
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
							/*Based on the newly calculated nS, it is possible to calculate the
							liquid and solid composition of n-Alkanes*/
							XL[i] = Z[i] / (1 + nS * (K[i] - 1));
							XS[i] = Z[i] * K[i] / (1 + nS * (K[i] - 1));
							if (XS[i] >(pow(10, -6)) && Click == 0) {
								Click = 1;
								MinC = i;
							}
							/*It saves the previous K values, so they can be compared with the new
							ones which will be estimated in the following section.*/
							KC[i] = K[i];
						}
					}

					//Equilibirum constant update
#pragma omp parallel for
					for (int i = MinC; i < CompNum + 1; i++)
					{
						if (Z[i] != 0)
						{
							K[i] = (GammaLiqMesEFV(MinC, i, CarbonNum[i], CompNum, CarbonNum, XL, T) /
								GammaSolMes(MinC, CarbonNum[i], CompNum, CarbonNum, XS, T, CF))
								*exp(((1000 * HeatFus(CarbonNum[i]) / (R*TempFus(CarbonNum[i])))
									*(TempFus(CarbonNum[i]) / T - 1)) + ((1000 * HeatTrans(CarbonNum[i]) /
									(R*TempTrans(CarbonNum[i])))*(TempTrans(CarbonNum[i]) / T - 1)) -
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
					Err = 0;
#pragma omp parallel for reduction (+:Err)
					for (int jk = MinC; jk < CompNum + 1; jk++)
					{
						Err = Err + sumErr[jk] / (CompNum - MinC);
					}

				}
				Err = 200;

				//WAT conidition to be satisfied
				if (nS < 0) {
					WATTInput = T;
					break;
				}
			}
		}

		if (nS < 0)
		{
			break;
		}
		Count = Count + 1;

		/*Please refer to parameter definition section to know the nature of
		each paramater*/
		SumW = 0;
#pragma omp parallel for reduction (+:SumW)
		for (int i = 1; i < CompNum + 1; i++)
		{
			SumW = SumW + nS * XS[i] * (12 * CarbonNum[i] + CarbonNum[i] * 2 + 2);
			if (XS[i] >(6 * pow(10, -5)) && Click == 0)
			{
				Click = 1;
				MinC = i;
			}
		}
		t = clock() - t;

		/*Second column of nSFile.txt is the weight fraction of solid phase in the
		total oil sample (n-alkanes + non n-alkanes) */
		if (nS > 0)
		{
			nSFile << setprecision(8) << fixed << NalkaneWeightFraction * SumW / SumWOil
				<< endl;
			Temperature << setprecision(2) << fixed << T << endl;
		}
		Err = 200;

		//The parameters which will be displayed at time of simulation run
		if (nS > 0)
		{
			cout << setprecision(2) << fixed << T << setw(10) << left << " " <<
				setprecision(4) << ((float)t) / CLOCKS_PER_SEC << setw(10) << left
				<< " " << setprecision(8) << fixed << NalkaneWeightFraction * SumW
				/ SumWOil << endl;
		}
	}

	//Solid weight composition at all temperatures
	for (int j = 0; j < NumTemp; j++)
	{
		for (int i = 1; i < CompNum + 1; i++)
		{
			OutSolidW << SolidCompW[j][i] << " ";
		}
		OutSolidW << endl;
	}

	//Wax Appearance Temperature (WAT) reporting
	if ((Count - 2) < NumTemp && nS<0)
	{
		WATT << Count << endl;
		cout << "WAT: " << WATTInput << " ";
		WATT << WATTInput;
	}
	else
	{
		WATT << Count << endl << 0;
		cout << "WAT has not reached, please choose higher temperature";
	}

	cout << endl;
	cout << "done!" << endl;

	nSFile.close();
	Temperature.close();
	OutSolidW.close();
	WATT.close();
}
