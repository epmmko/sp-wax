
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* * * * * *                  Coutinho                 * * * * * *
* * * * * *          Solid-liquid Equilibrium         * * * * * *
* * * * * *              Thermodynamic Model          * * * * * *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

//In this file, functions and int main are combined

#include <iostream>
#include <cmath>
#include <fstream>
#include <windows.h>
#include <string>


using namespace std;

//universal gas constant
const double R = 8.3144621; // J/Mol/K -All units are in SI 

//Number of components
const int CompNum = 2;

//Current directory is called
string ExeDir() {
	char Var[MAX_PATH];
	GetModuleFileName(NULL, Var, MAX_PATH);
	string::size_type pos = string(Var).find_last_of("\\/");
	return string(Var).substr(0, pos);
}

/*Heat of fusion correlation suggested by Coutinho
checked by experimental data
Unit = [KJ/mol]
*/
double HeatFus(int CN)  //Coutinho's Correlation [KJ/mol] 
{
	return ((0.00355*CN*CN*CN - 0.2376*CN*CN + 7.4*CN - 34.814));
}

/*Heat of solid phase transition correlation suggested by Coutinho
checked by experimental data
Unit = [KJ/mol]
*/
double HeatTrans(int CN)
{
	double Dhf, Dhtot, Dht2;
	if (CN > 8)
	{
		Dhf = 0.00355*CN*CN*CN - 0.2376*CN*CN + 7.4*CN - 34.814;
		Dhtot = 3.7791*CN - 12.654;
		Dht2 = (Dhtot - Dhf);
	}
	else
	{
		Dht2 = 0;
	}

	return (Dht2);
}

/*Heat of vaporization using extension of Pitzer CSP models
checked by experimental data and other correlations
High uncertainty, even between experimental data
Unit = [KJ/mol]
*/
double HeatVap(double T, int CN)
{
	// Checked and verfied with experimental data
	double Tc;
	double x;
	double Dh0Star;
	double Dh1Star;
	double Dh2Star;
	double Omega;
	double DhStar;
	double Dh;

	Tc = 959.95 - exp(6.81536 - 0.211145*(pow(CN, (2.0 / 3.0))));
	x = 1 - T / Tc;
	Dh0Star = 5.2804*(pow(x, 0.333)) + 12.865*(pow(x, 0.8333)) + 1.171*(pow(x, 1.2083)) 
		- 13.116*x + 0.4858*x*x - 1.088*x*x*x;
	Dh1Star = 0.80022*(pow(x, 0.333)) + 273.23*(pow(x, 0.8333)) + 465.08*(pow(x, 1.2083))
		- 638.51*x - 145.12*x*x + 74.049*x*x*x;
	Dh2Star = 7.2543*(pow(x, 0.333)) - 346.45*(pow(x, 0.8333)) - 610.48*(pow(x, 1.2083))
		+ 839.89*x + 160.05*x*x - 50.711*x*x*x;
	Omega = 0.052075 + 0.0448946*CN - 0.000185397*CN*CN;
	DhStar = Dh0Star + Dh1Star * Omega + Dh2Star * Omega*Omega;
	Dh = (DhStar*Tc*R) / 1000.0;

	return(Dh);
}

/*Temperature of fusion suggested by Coutinho
unit = [KJ/mol]
*/
double TempFus(int CN)
{
	return((421.63 - (421.63 + 1935991)*exp(-7.8945*(pow((CN - 1), 0.07194)))));
}

/*Temperature of solid phase transition suggested by Coutinho
unit = [KJ/mol]
*/
double TempTrans(int CN)
{
	return(420.42 - (420.42 + 134364)*exp(-4.344*pow((CN + 6.592), 0.14627)));
}

/*Molar volume of each carbon number component in liquid phase
using DIPPR correlations and GCVOL group contribution method
checked by experimental data
unit = [m3/mol]
*/
double Vm(double T, int CN) // [m3/Kmol]
{
	string Directory = ExeDir();
	double V_m; //Return
	double A[14], B[14], C[14], D[14];
	double a, b, c, d, CH3_A = 18.96, CH3_B = 45.58, CH2_A = 12.52, CH2_B = 12.94;
	ifstream DIPP;
	DIPP.open(Directory + "\\DIPP.txt");;

	if ((CN) < 21 && (CN) > 6) // For carbon numbers [7-20] direct values from DIPPR's Corrlation
	{
		for (int j = 0; j < 14; j++) {
			DIPP >> A[j];
		}
		for (int j = 0; j < 14; j++) {
			DIPP >> B[j];
		}
		for (int j = 0; j < 14; j++) {
			DIPP >> C[j];
		}
		for (int j = 0; j < 14; j++) {
			DIPP >> D[j];
		}
		a = A[CN - 7];
		b = B[CN - 7];
		c = C[CN - 7];
		d = D[CN - 7];
		V_m = 1.0 / (a / (pow(b, 1 + pow(1 - T / c, d))));
	}
	else //For heavier components, GCVOL uses group increment method as follows
	{
		V_m = ((CN - 2.0)*(CH2_A + 0.001*CH2_B*T) + 2 * (CH3_A + 0.001*CH3_B*T)) / 1000.0;
	}
	return(V_m / 1000.0);
}

/*
Van Der Waals volume from Bondi(1989)
increment method
unit = [m3/mol]
*/
double Vw(int CN) // [m3/Kmol]
{
	double V_w = (2 * 13.67 + (CN - 2)*10.23) / 1000.0;
	return(V_w / 1000.0);
}

/*
interaction energy parameters
λ_ii,λ_jj & λ_ij
Correlation type 3
λ_ij=α_ij min⁡(λ_ii,λ_jj)
where α_ij is 1.0
*/
double Lambda(int CN1, int CN2, double T) //Energy interaction
{
	//Heat of sublimation
	double DhSub1;
	double DhSub2;
	double Lambii;
	double Lambjj;
	double Lambdaij;
	double Ans;
	double Alphaij=1.0;
	DhSub1 = (HeatVap(T, CN1) + HeatTrans(CN1) + HeatFus(CN1)) * 1000; // [J/mol] 
	DhSub2 = (HeatVap(T, CN2) + HeatTrans(CN2) + HeatFus(CN2)) * 1000; // [J/mol]

 //interaction energy parameters
	Lambii = (-1.0 / 3.0)*(DhSub1 - R * T);
	Lambjj = (-1.0 / 3.0)*(DhSub2 - R * T);
	if (CN1 < CN2)
	{
		Lambdaij = Alphaij * Lambii;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else if (CN1 != CN2)
	{
		Lambdaij = Alphaij * Lambjj;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else {
		Ans = 1;
	}
	return (Ans);
}

/*
Non-ideality of solid phase (activity coefficients, Gamma_Solid)
Wilson methodology
This function is primarily used for binary systems and has also been verified for multicomponent systems
*/
double GammaSolMes(int MinC, int CN1, int CompNum, int CarbonNum[], double xS[], double T) 																   
{ 
	double sumE = 0;
	for (int CN2 = MinC; CN2 < (CompNum + 1); CN2++) {
		sumE = sumE + Lambda(CN1, CarbonNum[CN2], T)*xS[CN2];  
	}
	double sum1 = 0;
	for (int CN3 = MinC; CN3 < (CompNum + 1); CN3++) {
		double sum2 = 0;
		for (int CN4 = MinC; CN4 < (CompNum + 1); CN4++) {
			sum2 = sum2 + Lambda(CarbonNum[CN3], CarbonNum[CN4], T)*xS[CN4];
		}
		sum1 = sum1 + Lambda(CarbonNum[CN3], CN1, T)*xS[CN3] / sum2;
	}
	return(exp(-log(sumE) + 1 - sum1));
}

/*
Entropic - FV model
Non-ideality of liquid phase(activity coefficients, Gamma_Liquid)
*/
double GammaLiqMes(int MinC, int NumOfCN1, int CN1, int CompNum, int CarbonNum[], double xL[], double T) // For binary system, the other liquid activity coefficient is used
{
	double ans;
	double Phi;
	double sum_L = 0;
	for (int j = 1; j < CompNum + 1; j++) {
		sum_L = sum_L + xL[j] * (pow(((pow(Vm(T, CarbonNum[j]), (1.0))) - (pow(Vw(CarbonNum[j]), (1.0)))), ((2.0 / 3.0))));
	}

	Phi = xL[NumOfCN1] * (pow(((pow(Vm(T, CN1), (1.0))) - (pow(Vw(CN1), (1.0)))), (2.0 / 3.0))) / sum_L;
	ans = exp(log(Phi / xL[NumOfCN1]) + 1 - Phi / xL[NumOfCN1]);
	return(ans);

}

//Material balance relation to calculate correct nS based on provided K-values
double objFunc(double nS, int CompNum, double Z[], double K[]) //This fucntion is used for finding the solid phase mole fraction
{
	double sum = 0;
	for (int i = 1; i < CompNum + 1; i++) {
		sum = sum + Z[i] * (K[i] - 1) / (1 + nS * (K[i] - 1));
	}
	return(sum);
}

//The following two functions are used to impliment	Newton Raphson method
double objFuncDer(double nS, int CompNum, double Z[], double K[]) //For Newthon Raphson method
{
	double dnS = 0.0000001;
	return((objFunc(nS + dnS, CompNum, Z, K) - objFunc(nS, CompNum, Z, K)) / dnS);

}
double Fsolve(double nS, int CompNum, double Z[], double K[]) //Newthon Raphson to find the root
{
	double nS1, nS2;
	nS1 = nS; //initial guess
	double err = 10;
	while (err > pow(10, -8))
	{
		nS2 = nS1 - (objFunc(nS1, CompNum, Z, K)) / (objFuncDer(nS1, CompNum, Z, K));
		err = abs(nS2 - nS1);
		nS1 = nS2;
	}
	return(nS2);
}

 /* this function reports solid mole fraction of wax for binary system.
 This function is dependent on mole fraction of solute and temperature*/
double BinarySys(double SoluteFrac, double T_Obj)
{
	string Directory = ExeDir();
	ifstream BinaryInfo, Composition;
	//Solute and solvent mole carbon numbers are assigned
	BinaryInfo.open(Directory + "\\BinaryInfo.txt");

	//Solvent and solute mole fractions
	double Z[CompNum + 1];

	//Equilibrium constants
	double K[CompNum + 1];

	//Equilibrium constant from previous iteration 
	double KC[CompNum + 1];

	//Compositions of liquid and solid phases
	double xL[CompNum + 1], xS[CompNum + 1];

	//Carbon numbers
	int CarbonNum[CompNum + 1];

	//Error
	double err = 10, sum_err;

	//Mole fraction of solidified wax in total fluid
	double nS = 0.00003;

	//Mole fraction of liquid phase
	double nL;

	//Useful parameters
	double T,MinC = 1;
	
	//Activity coefficients for liquid and solid phases
	double GL;
	double GS;

	//Reading solvent and solute carbon numbers from the text file
	BinaryInfo >> CarbonNum[1];
	BinaryInfo >> CarbonNum[2];

	Z[2] = SoluteFrac;
	Z[1] = 1 - Z[2];
	K[0] = 0;
	K[1] = 0.01;
	K[2] = 1600;


	//ns is calculated through the following while-loop
	T = T_Obj;
	while (err > pow(10, -8))
	{
		nS = Fsolve(nS, CompNum, Z, K); 
		nL = 1 - nS;

		for (int i = 1; i < CompNum + 1; i++)
		{
			xL[i] = Z[i] / (1 + nS * (K[i] - 1));
			xS[i] = Z[i] * K[i] / (1 + nS * (K[i] - 1));
			KC[i] = K[i];
		}

		sum_err = 0;
		for (int i = 1; i < CompNum + 1; i++)
		{
			GS = GammaSolMes(1.0, CarbonNum[i], CompNum, CarbonNum, xS, T);
			GL = GammaLiqMes(1.0, i, CarbonNum[i], CompNum, CarbonNum, xL, T);
			K[i] = (GL / GS)*exp((1000 * HeatFus(CarbonNum[i]) / 
				(R*TempFus(CarbonNum[i])))*(TempFus(CarbonNum[i]) / T - 1) + 
				(1000 * HeatTrans(CarbonNum[i]) / (R*TempTrans(CarbonNum[i])))*
				(TempTrans(CarbonNum[i]) / T - 1));
			sum_err = sum_err + abs(K[i] - KC[i]); 
		}

		err = sum_err;

	}

	return(nS);
}

//This function is used for Newton Raphson method which is included in int main
double BinarySysDer(double SoluteFrac, double T_Obj)
{
	double dx = pow(10, -8);
	double ans = (BinarySys(SoluteFrac, T_Obj + dx) - BinarySys(SoluteFrac, T_Obj)) / dx;
	return(ans);
}


int main()
{
	//current directory is called
	string Directory = ExeDir();

	//iterative integer
	int i, counter = 0;

	//Useful parameter (no specific meaning)
	double a=1;

	//Solute mole fraction assignment
	double FracParam[100];

	//Temperature steps
	double step1 = 5;
	double step = 2;


	//Error 
	double err = 20;

	//Initial temperature
	double T = 260;

	//Solute mole fraction values are read from text file
	ifstream SoluteFractionFile;
	SoluteFractionFile.open(Directory + "\\SoluteFractionFile.txt");

	/*Wax Appearance Temperature values for corresponding solute mole fractions
	arecalculated and exported to textfile as outputs*/
	ofstream Out;
	Out.open(Directory + "\\OutPutBinary.txt");

	//Simulation results are shown at the time of running
	cout << "Solute fraction		WAT" << endl;

	//Solute mole fraction values are assigned to the array
	for (i = 0; i < 100; i++) {
		SoluteFractionFile >> a;
		counter = i;
		if (a == 1) {
			break;
		}
		counter = i + 1;

		FracParam[i] = a;
	}

	//WAT  for all solute mole fractions are calculated by this for loop 
	for (int j = 0; j < counter; j++) {
		err = 20;
		T = 260;

		/*The temperature that does not result in nan value is detected here
		in this while loop*/
		while (isnan(BinarySys(FracParam[j], T)))
		{
			T = T + step1;
			a = BinarySys(FracParam[j], T);
		}

		/*A new temperature step is used to move forward
		to find WAT (by two following while loops)*/
		step = 1;
		a = BinarySys(FracParam[j], T);
		while (abs(BinarySys(FracParam[j], T)) > 0.0001)
		{
			T = T + step;
			a = BinarySys(FracParam[j], T);
			if (BinarySys(FracParam[j], T) < 0 || isnan(BinarySys(FracParam[j], T))) {
				T = T - step;
				break;
			}
		}

		a = BinarySys(FracParam[j], T);
		while (abs(err) > pow(10, -9))
		{
			T = T - BinarySys(FracParam[j], T) / BinarySysDer(FracParam[j], T);
			err = BinarySys(FracParam[j], T);
		}

		cout << FracParam[j] << "			" << T << endl;
		Out << T << endl;
	}

	cin >> T;
	Out.close();
}