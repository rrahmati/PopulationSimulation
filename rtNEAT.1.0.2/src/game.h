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

	int NUM_RAYCASTS = 3;
	int NUM_INPUTS = NUM_RAYCASTS * 3 + 1;
	int NUM_OUTPUTS = 3;

	Population *game_test(int gens);
	bool game_progress(Organism *org);
	bool donation_eligibility(Organism *org ,Organism *org2);
	int game_epoch(Population *pop,int generation,char *filename, int &winnernum, int &winnergenes,int &winnernodes, 		double &fitness, int &species, int &genes);
	void create_agent(Organism * org);
	int Game_realtime_loop(Population *pop);
	bool take_action(Organism *org);
	void test_time();
public:
	Population *Game_test_realtime();

};
#endif
