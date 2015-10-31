#include "game.h"
#include <cstring>
#include "neatmain.h"

bool Org_fitness(Organism *org, double age, double food_gain, int wall_hit){
		 double fitness = (age * (food_gain))/wall_hit;

		 org->fitness = fitness;
	 if(org-> fitness > 5./*win_threshold*/)
		 return true; // recognize this org as winner
	 return false;

}
double alt_penalize(double giver_old_food_level, double food_granted, double rec_old_food_level, double r){
			double giver_curr_food_level = giver_old_food_level - food_granted;
			double rec_curr_food_level = rec_old_food_level + food_granted;
			// compute the altruism cost for this org upon this particular help
			if(food_granted == 0.)
				return 0;
			double c = 0;
			if(giver_curr_food_level == 0. && food_granted > 0)
				c = 100000.; // this agent killed itself for that agent or was in critical point --> should not help
			else c = 1./ giver_curr_food_level;
			double b = 1./ (rec_curr_food_level);
			cout << c <<" "<< r <<" " << b <<" " << c/r << " "<< Hamilton_rate << endl;
			if(b > c/r)
				return 0.; // the rule meets

			return ( b - c/r ) * Hamilton_rate;

}

void org_evaluate() {
	double f;
	cout << "reading input file.";
	ifstream iFile("NNinput",ios::in);
	iFile>>f;

			iFile.close();
	cout << f;
}
bool Game_evaluate(Organism *org){

	// should decide whether that org is a winner or not. Since we have no winner I return false.
	return false;

}
// *pole2_test_realtime is employed
Population *Game_test_realtime() {
	Population *pop;
	Genome *start_genome;
	char curword[20];
	int id;

	ostringstream *fnamebuf;
	int gen;
	//CartPole *thecart;

	double highscore;

	ifstream iFile("gamestartgenes", ios::in);

	cout << "START Game REAL-TIME EVOLUTION VALIDATION"
			<< endl;

	cout << "Reading in the start genome" << endl;
	//Read in the start Genome
	iFile >> curword;
	iFile >> id;
	cout << "Reading in Genome id " << id << endl;
	start_genome = new Genome(id, iFile);
	iFile.close();

	cout << "Start Genome: " << start_genome << endl;

	//Spawn the Population from starter gene
	cout << "Spawning Population off Genome" << endl;
	pop = new Population(start_genome, NEAT::pop_size);

	//Alternative way to start off of randomly connected genomes
	//pop=new Population(pop_size,7,1,10,false,0.3);

	cout << "Verifying Spawned Pop" << endl;
	pop->verify();

	//Create the Cart
	//thecart = new CartPole(true, 1);

	//Start the evolution loop using rtNEAT method calls
	highscore = Game_realtime_loop(pop/*, thecart*/);

	return pop;
}

int Game_realtime_loop(Population *pop /*, CartPole *thecart*/) {
	vector<Organism*>::iterator curorg;
	vector<Species*>::iterator curspecies;

	vector<Species*>::iterator curspec; //used in printing out debug info

	vector<Species*> sorted_species; //Species sorted by max fit org in Species

	int pause;
	bool win = false;

	double champ_fitness;
	Organism *champ;

	//double statevals[5]={-0.9,-0.5,0.0,0.5,0.9};
	double statevals[5] = { 0.05, 0.25, 0.5, 0.75, 0.95 };

	int s0c, s1c, s2c, s3c;

	int score;

	//Real-time evolution variables
	int offspring_count;
	Organism *new_org;

	//thecart->nmarkov_long = false;
	//thecart->generalization_test = false;

	//We try to keep the number of species constant at this number
	int num_species_target = NEAT::pop_size / 15;

	//This is where we determine the frequency of compatibility threshold adjustment
	int compat_adjust_frequency = NEAT::pop_size / 10;
	if (compat_adjust_frequency < 1)
		compat_adjust_frequency = 1;

	//Initially, we evaluate the whole population
	//Evaluate each organism on a test
	for (curorg = (pop->organisms).begin(); curorg != (pop->organisms).end();
			++curorg) {

		//shouldn't happen
		if (((*curorg)->gnome) == 0) {
			cout << "ERROR EMPTY GEMOME!" << endl;
			cin >> pause;
		}

		if (Game_evaluate((*curorg)/*, 1, thecart*/))
			win = true;

	}

	//Get ready for real-time loop

	//Rank all the organisms from best to worst in each species
	pop->rank_within_species();

	//Assign each species an average fitness
	//This average must be kept up-to-date by rtNEAT in order to select species probabailistically for reproduction
	pop->estimate_all_averages();

	//Now create offspring one at a time, testing each offspring,
	// and replacing the worst with the new offspring if its better
	for (offspring_count = 0; offspring_count < 20000; offspring_count++) {

		//Every pop_size reproductions, adjust the compat_thresh to better match the num_species_targer
		//and reassign the population to new species
		if (offspring_count % compat_adjust_frequency == 0) {

			int num_species = pop->species.size();
			double compat_mod = 0.1; //Modify compat thresh to control speciation

			// This tinkers with the compatibility threshold
			if (num_species < num_species_target) {
				NEAT::compat_threshold -= compat_mod;
			} else if (num_species > num_species_target)
				NEAT::compat_threshold += compat_mod;

			if (NEAT::compat_threshold < 0.3)
				NEAT::compat_threshold = 0.3;

			cout << "compat_thresh = " << NEAT::compat_threshold << endl;

			//Go through entire population, reassigning organisms to new species
			for (curorg = (pop->organisms).begin();
					curorg != pop->organisms.end(); ++curorg) {
				pop->reassign_species(*curorg);
			}
		}

		//For printing only
		for (curspec = (pop->species).begin(); curspec != (pop->species).end();
				curspec++) {
			cout << "Species " << (*curspec)->id << " size"
					<< (*curspec)->organisms.size() << " average= "
					<< (*curspec)->average_est << endl;
		}

		cout << "Pop size: " << pop->organisms.size() << endl;

		//Here we call two rtNEAT calls:
		//1) choose_parent_species() decides which species should produce the next offspring
		//2) reproduce_one(...) creates a single offspring fromt the chosen species
		new_org = (pop->choose_parent_species())->reproduce_one(offspring_count,
				pop, pop->species);

		//Now we evaluate the new individual
		//Note that in a true real-time simulation, evaluation would be happening to all individuals at all times.
		//That is, this call would not appear here in a true online simulation.
		cout << "Evaluating new baby: " << endl;
		if (Game_evaluate(new_org/*, 1, thecart*/))
			win = true;

		if (win) {
			cout << "WINNER" << endl;
			pop->print_to_file_by_species("rt_winpop");
			break;
		}

		//Now we reestimate the baby's species' fitness
		new_org->species->estimate_average();

		//Remove the worst organism
		pop->remove_worst();

	}

}
