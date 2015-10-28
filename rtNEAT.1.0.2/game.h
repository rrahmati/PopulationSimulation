
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

using namespace std;

using namespace NEAT;

//The 3-game evolution routines *****************************************
Population *game_test(int gens);
bool game_evaluate(Organism *org);
int game_epoch(Population *pop,int generation,char *filename, int &winnernum, int &winnergenes,int &winnernodes, double &fitness, int &species, int &genes);

void readWriteFile();

