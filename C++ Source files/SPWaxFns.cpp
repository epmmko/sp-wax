
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* * * * * *                  Coutinho                 * * * * * *
* * * * * *          Solid-liquid Equilibrium         * * * * * *
* * * * * *              Thermodynamic Model          * * * * * *
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

/*This .CPP file contains the functions which are used in the model. 
Please be adviced that some functions are defined here and has not been used
in the main cocde. However, those functions are potential substitutes which
could, also, be used. */

//Used libraries
#include <iostream>
#include <cmath>
#include <fstream>
#include <time.h>
#include <iomanip> 
#include "omp.h"
#include <windows.h>
#include <string>
using namespace std;

//Some global variables 
double A[14];
double B[14];
double C[14];
double D[14];

//Ideal gas constant
const double R = 8.3144621; // J/Mol/K -All units are in SI 

//This function returns the 
string ExeDire() {
	char Var[MAX_PATH];
	GetModuleFileName(NULL, Var, MAX_PATH);
	string::size_type pos = string(Var).find_last_of("\\/");
	return string(Var).substr(0, pos);
}

/*Fusion temperature correlation from Yang et al.
This correlation is not suggested by Coutinho and it is not primarily 
used however, it is checked by experimental data and shows good accuracy,
Unit = [Kelvin]
*/
double TempFusYang(int CN)
{
	double Ans;
	double Mw = 12 * CN + 2 * CN + 2;

	if (Mw<451)
	{
		Ans = 374.5 + 0.02617*Mw - 20172 / Mw;
	}
	else
	{
		Ans = 411.4 - 32326 / Mw;
	}

	return(Ans);
}


/*Heat of fusion correlation from Yang et al.
This correlation is not suggested by Coutinho and it is not primarily used however,
it is checked by experimental data and shows good accuracy,
Unit = [KJ/Mol]
*/
double HeatFusYang(int CN)
{
	double Ans;
	int Prod;
	Prod = CN / 2.0;
	double Mw = 12 * CN + 2 * CN + 2;
	if ((CN >= 7) && (CN <= 22))
	{
		if (abs(CN - Prod * 2.0)<0.00001)
		{
			Ans = (0.8064*Mw*TempFusYang(CN));
		}
		else
		{
			Ans = (0.5754*Mw*TempFusYang(CN));
		}
	}
	else if ((CN > 22) && (CN < 38))
	{
		Ans = 0.4998*Mw*TempFusYang(CN);
	}
	else
	{
		Ans = 0.674*Mw*TempFusYang(CN);
	}
	return(Ans / 1000.0);
}


/*Heat of fusion correlation suggested by Coutinho
checked by experimental data
Unit = [KJ/mol]
*/
double HeatFus(int CN)
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
	//[KJ/mol]
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
	Dh0Star = 5.2804*(pow(x, 0.333)) + 12.865*(pow(x, 0.8333)) + 1.171*(pow(x, 1.2083)) - 13.116*x + 0.4858*x*x - 1.088*x*x*x;
	Dh1Star = 0.80022*(pow(x, 0.333)) + 273.23*(pow(x, 0.8333)) + 465.08*(pow(x, 1.2083)) - 638.51*x - 145.12*x*x + 74.049*x*x*x;
	Dh2Star = 7.2543*(pow(x, 0.333)) - 346.45*(pow(x, 0.8333)) - 610.48*(pow(x, 1.2083)) + 839.89*x + 160.05*x*x - 50.711*x*x*x;
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


//Following four functions are used to call DIPPR coefficients of molar volume correlations
double *ArrayRetA()
{
	string Directory = ExeDire();
	ifstream DIPP;

	DIPP.open(Directory +"\\DIPP.txt");
	for (int j = 0; j < 14; j++) {
		DIPP >> A[j];
	}
	return(A);
}
double *ArrayRetB()
{
	string Directory = ExeDire();
	double a;
	ifstream DIPP;
	DIPP.open(Directory + "\\DIPP.txt");
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> B[j];
	}
	return(B);
}
double *ArrayRetC()
{
	string Directory = ExeDire();
	double a;
	ifstream DIPP;
	DIPP.open(Directory + "\\DIPP.txt");
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> C[j];
	}
	return(C);
}
double *ArrayRetD()
{
	string Directory = ExeDire();
	double a;
	ifstream DIPP;
	DIPP.open(Directory + "\\DIPP.txt");
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> a;
	}
	for (int j = 0; j < 14; j++) {
		DIPP >> D[j];
	}
	return(D);
}


/*Molar volume of each carbon number component in liquid phase
using DIPPR correlations and GCVOL group contribution method
checked by experimental data
unit = [m3/mol]
*/
double Vm(double T, int CN)
{
	double Vm;
	double a, b, c, d;

	//GCVOL group increment method constants
	double Ch3A = 18.96, Ch3B = 45.58, Ch2A = 12.52, Ch2B = 12.94;

	//DIPPR correlations
	if ((CN) < 21 && (CN) > 6)
	{
		a = A[CN - 7];
		b = B[CN - 7];
		c = C[CN - 7];
		d = D[CN - 7];
		Vm = 1.0 / (a / (pow(b, 1 + pow(1 - T / c, d))));
	}
	//GCVOL group increment method
	else
	{
		Vm = ((CN - 2.0)*(Ch2A + 0.001*Ch2B*T) + 2 * (Ch3A + 0.001*Ch3B*T)) / 1000.0;
	}
	return(Vm / 1000.0);
}


/*
Van Der Waals volume from Bondi(1989)
increment method
unit = [m3/mol]
*/
double Vw(int CN)
{
	double VolW;
	VolW = (2 * 13.67 + (CN - 2)*10.23) / 1000.0;
	return(VolW / 1000.0);
}


/*
interaction energy parameters
λ_ii,λ_jj & λ_ij
Correlation type 3
λ_ij=α_ij min⁡(λ_ii,λ_jj)
*/
double LambdaCorr3(int CN1, int CN2, double T, double CF)
{
	//Heat of sublimation
	double DhSub1;
	double DhSub2;
	double Lambii;
	double Lambjj;
	double Lambdaij;
	double Ans;
	double Alphaij;
	DhSub1 = (HeatVap(T, CN1) + HeatTrans(CN1) + HeatFus(CN1)) * 1000; // [J/mol] 
	DhSub2 = (HeatVap(T, CN2) + HeatTrans(CN2) + HeatFus(CN2)) * 1000; // [J/mol]

	//interaction energy parameters
	Lambii = (-1.0 / 3.0)*(DhSub1 - R * T);
	Lambjj = (-1.0 / 3.0)*(DhSub2 - R * T);
	
	//interaction energy parameters between long and short n-alkanes
	Alphaij = 1 - CF * (abs(DhSub1 - DhSub2));
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
interaction energy between short and long n-alkane molecules
λ_ii,λ_jj & λ_ij
Correlation type 1
λ_ij=(1+α_ij)λ_short
*/
double LambdaCorr1(int CN1, int CN2, double T, double CF) //Energy interaction
{
	//Heat of sublimation
	double DhSub1;
	double DhSub2;
	double Lambii;
	double Lambjj;
	double Lambdaij;
	double Ans;
	double Alphaij;
	double Ls, Ll;
	DhSub1 = (HeatVap(T, CN1) + HeatTrans(CN1) + HeatFus(CN1)) * 1000; // [J/mol] 
	DhSub2 = (HeatVap(T, CN2) + HeatTrans(CN2) + HeatFus(CN2)) * 1000; // [J/mol]

	//interaction energy parameters
	Lambii = (-1.0 / 3.0)*(DhSub1 - R * T);
	Lambjj = (-1.0 / 3.0)*(DhSub2 - R * T);

	//interaction energy parameters between long and short n-alkanes
	if (CN1 < CN2)
	{
		Ls = 1.27*CN1 + 1.98;
		Ll = 1.27*CN2 + 1.98;
		Alphaij = -73.98*((pow((Ll - Ls), 2)) / (pow(Ls, 3))) + 0.01501;
		Lambdaij = (1 + Alphaij)*Lambii;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else if (CN1 != CN2)
	{
		Ll = 1.27*CN1 + 1.98;
		Ls = 1.27*CN2 + 1.98;
		Alphaij = -73.98*((pow((Ll - Ls), 2)) / (pow(Ls, 3))) + 0.01501;
		Lambdaij = (1 + Alphaij)*Lambjj;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else {
		Ans = 1;
	}
	return (Ans);
}

/*
interaction energy between short and long n-alkane molecules
λ_ii,λ_jj & λ_ij
Correlation type 2
λ_ij=(1+α_ij)λ_short
*/
double LambdaCorr2(int CN1, int CN2, double T, double CF) //Energy interaction
{
	//Heat of sublimation
	double DhSub1;
	double DhSub2;
	double Lambii;
	double Lambjj;
	double Lambdaij;
	double Ans;
	double Alphaij;
	double Ls, Ll;
	DhSub1 = (HeatVap(T, CN1) + HeatTrans(CN1) + HeatFus(CN1)) * 1000; // [J/mol] 
	DhSub2 = (HeatVap(T, CN2) + HeatTrans(CN2) + HeatFus(CN2)) * 1000; // [J/mol]

	//interaction energy parameters
	Lambii = (-1.0 / 3.0)*(DhSub1 - R * T);
	Lambjj = (-1.0 / 3.0)*(DhSub2 - R * T);

	if (CN1 < CN2)
	{
		Ls = 1.27*CN1 + 1.98;
		Ll = 1.27*CN2 + 1.98;
		if (abs(CN1 - CN2) <3) 
		{
			Alphaij = -73.98*((pow((Ll - Ls), 2)) / (pow(Ls, 3))) + 0.01501;
		}
		else 
		{
			Alphaij = 0.00227*(Ll - Ls) - 0.222;
		}
		Lambdaij = (1 + Alphaij)*Lambii;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else if (CN1 != CN2)
	{
		Ll = 1.27*CN1 + 1.98;
		Ls = 1.27*CN2 + 1.98;
		if (abs(CN1 - CN2) < 3) {
			Alphaij = -73.98*((pow((Ll - Ls), 2)) / (pow(Ls, 3))) + 0.01501;
		}
		else 
		{
			Alphaij = 0.00227*(Ll - Ls) - 0.222;
		}
		Lambdaij = (1 + Alphaij)*Lambjj;
		Ans = exp(-(Lambdaij - Lambii) / (R*T));
	}
	else 
	{
		Ans = 1;
	}

	return (Ans);
}

/*
Non-ideality of solid phase (activity coefficients, Gamma_Solid)
Wilson methodology
This function is primarily used for multicomponent systems
*/
double GammaSolid(int CN1, double Xs[], int MinC, int MaxC, double T, double CF)
{
	double SumL = 0;
	double SumE = 0;
	double Sum1 = 0;
	double Sum2 = 0;

	for (int CN2 = MinC; CN2 < (MaxC + 1); CN2++) 
	{
		SumE = SumE + LambdaCorr3(CN1, CN2, T, CF)*Xs[CN2];
	}
	Sum1 = 0;
	for (int CN3 = MinC; CN3 < (MaxC + 1); CN3++) 
	{
		Sum2 = 0;
		for (int CN4 = MinC; CN4 < (MaxC + 1); CN4++) {
			Sum2 = Sum2 + LambdaCorr3(CN3, CN4, T, CF)*Xs[CN4];
		}
		Sum1 = Sum1 + LambdaCorr3(CN3, CN1, T, CF)*Xs[CN3] / Sum2;
	}
	return(exp(-log(SumE) + 1 - Sum1));
}


/*
Non-ideality of solid phase (activity coefficients, Gamma_Solid)
Wilson methodology
This function is primarily used for binary systems and has also been verified
for multicomponent systems
*/
double GammaSolMes(int MinC, int CN1, int CompNum, int CarbonNum[], double 
	Xs[], double T, double CF)
{
	double SumE = 0;
	double Sum1 = 0;
	double Sum2 = 0;
	for (int CN2 = MinC; CN2 < (CompNum + 1); CN2++) 
	{
		SumE = SumE + LambdaCorr3(CN1, CarbonNum[CN2], T, CF)*Xs[CN2];  //checkt eh CN2-1
	}

	Sum1 = 0;
	for (int CN3 = MinC; CN3 < (CompNum + 1); CN3++) 
	{
		double Sum2 = 0;
		for (int CN4 = MinC; CN4 < (CompNum + 1); CN4++) 
		{
			Sum2 = Sum2 + LambdaCorr3(CarbonNum[CN3], CarbonNum[CN4], T, CF)*Xs[CN4];
		}
		Sum1 = Sum1 + LambdaCorr3(CarbonNum[CN3], CN1, T, CF)*Xs[CN3] / Sum2;
	}
	return(exp(-log(SumE) + 1 - Sum1));
}



/*
Non-ideality of liquid phase (activity coefficients, Gamma_Liquid)
Flory - FV model
*/
double GammaLiqMesFFV(int MinC, int NumOfCN1, int CN1, int CompNum,
	int CarbonNum[], double XL[], double T)
{
	double SumL = 0;
	double Phi;
	for (int j = MinC - 1; j < CompNum + 1; j++) 
	{
		SumL = SumL + XL[j] * (pow(((pow(Vm(T, CarbonNum[j]), (1.0 / 3.0)))
			- (pow(Vw(CarbonNum[j]), (1.0 / 3.0)))), (3.3)));
	}
	Phi = XL[NumOfCN1] * (pow(((pow(Vm(T, CN1), (1.0 / 3.0))) - (pow(Vw(CN1)
		, (1.0 / 3.0)))), 3.3)) / SumL;
	return(exp(log(Phi / XL[NumOfCN1]) + 1 - Phi / XL[NumOfCN1]));
}


/*
Entropic - FV model
Non-ideality of liquid phase(activity coefficients, Gamma_Liquid)
*/
double GammaLiqMesEFV(int MinC, int NumOfCN1, int CN1, int CompNum,
	int CarbonNum[], double XL[], double T) // For binary system, the other liquid activity coefficient is used
{
	double SumL = 0;
	double Phi;
	for (int j = 1; j < CompNum + 1; j++) 
	{
		SumL = SumL + XL[j] * (pow(((pow(Vm(T, CarbonNum[j]), (1.0)))
			- (pow(Vw(CarbonNum[j]), (1.0)))), ((2.0 / 3.0))));
	}
	Phi = XL[NumOfCN1] * (pow(((pow(Vm(T, CN1), (1.0))) - (pow(Vw(CN1),
		(1.0)))), (2.0 / 3.0))) / SumL;
	return(exp(log(Phi / XL[NumOfCN1]) + 1 - Phi / XL[NumOfCN1]));
}


//Material balance relation to calculate correct nS based on provided K-values
double objFunc(double nS, int CompNum, double Z[], double K[])
{
	double Sum = 0;
	for (int i = 1; i < CompNum + 1; i++) 
	{
		Sum = Sum + Z[i] * (K[i] - 1) / (1 + nS * (K[i] - 1));
	}
	return(Sum);
}


//The following two functions are used to impliment	Newton Raphson method
double objFuncDer(double nS, int CompNum, double Z[], double K[])
{
	double dnS = 0.00000001;
	return((objFunc(nS + dnS, CompNum, Z, K) - objFunc(nS, CompNum, Z, K)) / dnS);

}
double Fsolve(double nS, int CompNum, double Z[], double K[])
{
	double nS1, nS2;
	nS1 = nS; //initial guess
	double Err = 10;
	while (Err > pow(10, -10))
	{
		nS2 = nS1 - (objFunc(nS1, CompNum, Z, K)) / (objFuncDer(nS1, CompNum, Z, K));
		Err = abs(nS2 - nS1);
		nS1 = nS2;
	}
	return(nS2);
}