#ifndef EXPERIMENTS_H
#define EXPERIMENTS_H

#include <iostream>
#include <iomanip>
#include <fstream>
#include <sstream>
#include <list>
#include <vector>
#include <algorithm>
#include <cmath>
#include <string>
#include "neat.h"
#include "network.h"
#include "population.h"
#include "organism.h"
#include "genome.h"
#include "species.h"

#define NO_SCREEN_OUT 0
using namespace std;

using namespace NEAT;

//The 3-parity evolution routines *****************************************
Population *parity_test(int gens);
bool parity_evaluate(Organism *org);
int parity_epoch(Population *pop,int generation,char *filename, int &winnernum, int &winnergenes,int &winnernodes, double &fitness, int &species, int &genes);

//The XOR evolution routines *****************************************
Population *xor_test(int gens);
bool xor_evaluate(Organism *org);
int xor_epoch(Population *pop,int generation,char *filename, int &winnernum, int &winnergenes,int &winnernodes, double &fitness, int &species, int &genes);

//Single pole balancing evolution routines ***************************
Population *pole1_test(int gens); 
bool pole1_evaluate(Organism *org);
int pole1_epoch(Population *pop,int generation,char *filename);
int go_cart(Network *net,int max_steps,int thresh); //Run input
//Move the cart and pole
void cart_pole(int action, float *x,float *x_dot, float *theta, float *theta_dot);

//Double pole balacing evolution routines ***************************
class CartPole;

Population *pole2_test(int gens,int velocity);
bool pole2_evaluate(Organism *org,bool velocity,CartPole *thecart);
int pole2_epoch(Population *pop,int generation,char *filename,bool velocity, CartPole *thecart,int &champgenes,int &champnodes, int &winnernum, ofstream &oFile);

//rtNEAT validation with pole balancing *****************************
Population *pole2_test_realtime();
int pole2_realtime_loop(Population *pop, CartPole *thecart);

class CartPole {
public:
  CartPole(bool randomize,bool velocity);
  virtual void simplifyTask();  
  virtual void nextTask();
  virtual double evalNet(Network *net,int thresh);
  double maxFitness;
  bool MARKOV;

  bool last_hundred;
  bool nmarkov_long;  //Flag that we are looking at the champ
  bool generalization_test;  //Flag we are testing champ's generalization

  double state[6];

  double jigglestep[1000];

protected:
  virtual void init(bool randomize);

private:

  void performAction(double output,int stepnum);
  void step(double action, double *state, double *derivs);
  void rk4(double f, double y[], double dydx[], double yout[]);
  bool outsideBounds(); 

  constexpr static int NUM_INPUTS=7;
  constexpr static double MUP = 0.000002;
  constexpr static double MUC = 0.0005;
  constexpr static double GRAVITY= -9.8;
  constexpr static double MASSCART= 1.0;
  constexpr static double MASSPOLE_1= 0.1;

  constexpr static double LENGTH_1= 0.5;		  /* actually half the pole's length */

  constexpr static double FORCE_MAG= 10.0;
  constexpr static double TAU= 0.01;		  //seconds between state updates

  constexpr static double one_degree= 0.0174532;	/* 2pi/360 */
  constexpr static double six_degrees= 0.1047192;
  constexpr static double twelve_degrees= 0.2094384;
  constexpr static double fifteen_degrees= 0.2617993;
  constexpr static double thirty_six_degrees= 0.628329;
  constexpr static double fifty_degrees= 0.87266;

  double LENGTH_2;
  double MASSPOLE_2;
  double MIN_INC;
  double POLE_INC;
  double MASS_INC;

  //Queues used for Gruau's fitness which damps oscillations
  int balanced_sum;
  double cartpos_sum;
  double cartv_sum;
  double polepos_sum;
  double polev_sum;



};

#endif






