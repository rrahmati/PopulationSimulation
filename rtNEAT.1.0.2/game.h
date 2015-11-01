#ifndef GAME_H
#define GAME_H

#include <iostream>
#include <iomanip>
#include <sstream>
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

using namespace std;

using namespace NEAT;

class Game {

	int NUM_RAYCASTS = 1;
	int NUM_INPUTS = NUM_RAYCASTS * 4 + 1;
	int NUM_OUTPUTS = NUM_RAYCASTS + 2;
	double Hamilton_rate = 10;


	Population *game_test(int gens);
	bool game_progress(Organism *org);
	int game_epoch(Population *pop,int generation,char *filename, int &winnernum, int &winnergenes,int &winnernodes, 		double &fitness, int &species, int &genes);
	void create_agent(Organism * org);
	bool org_fitness(Organism *org, double age, double food_gain, int wall_hit);
	double alt_penalize(double giver_old_food_level, double food_granted, double rec_old_food_level, double r);
	int Game_realtime_loop(Population *pop);
	bool org_evaluate(Organism *org);
public:
	Population *Game_test_realtime();

};
#endif
