
#include "game.h"
#include <thread>
#include <chrono>
#include <windows.h>
void Game::create_agent(Organism * org) {

}

bool Game::donation_eligibility(Organism *giver, Organism *receiver){ // check if giver can donate food to receiver
	// both orgs should belong to the same specious (also food level of giver should be higher)
	if(giver->gnome->genome_id == receiver->gnome->genome_id)
		return true;
	return false;
}

bool Game::take_action(Organism *org) {

	double input[NUM_INPUTS-1];
	double output[NUM_OUTPUTS];
	string infilename = "src\\in_out\\NNinput_";
	ostringstream ss;
	ss << org->gnome->genome_id;
	infilename = infilename + ss.str();
	if(!ifstream(infilename.c_str())) {
		create_agent(org);
		for(int i = 0; i < NUM_INPUTS-1; i++) {
			input[i] = 0;
		}
		org->fitness = 0;
	}

	ifstream iFile(infilename.c_str(), ios::in);
	// reading NN files first and keeping them in an array other than input

    //cout <<"%%%%%%  " <<NUM_INPUTS << endl;
    char c;
    double temp;
    vector<double> v;
	for(int i = 0; i < NUM_INPUTS; i++){
//		if((iFile >> temp) == 0)
//			cerr << "NUM_INPUTS is wrong or NNinput file" << endl;
		iFile >> temp;
		v.push_back(temp);
		iFile >> c;
	}
	iFile.close();

    // input of nueral network --> subject to change
	for(int i = 0; i < NUM_INPUTS - 1; i++)
        input[ i ] = v[ i ];
	org->fitness = v[ v.size() - 1 ]; // the last number in NN is fitness


	org->net->load_sensors(input);
	//Activate the net
	if (!(org->net->activate())) {
		org->fitness = -1000;
		return false;
	}

	string outfilename = "src\\in_out\\NNoutput_";
	outfilename = outfilename + ss.str();
	ofstream oFile(outfilename.c_str(), ios::out);
	for(int i = 0; i < NUM_OUTPUTS; i++) {
		output[i] = (*(org->net->outputs[i])).activation;
		oFile << output[i] << ",";
	}
	oFile.close();


	// should decide whether that org is a winner or not. Since we have no winner I return false.
	return false;

}
// *pole2_test_realtime is employed
Population* Game::Game_test_realtime() {
	Population *pop;
	Genome *start_genome;
	char curword[20];
	int id;

	ostringstream *fnamebuf;
	int gen;

	double highscore;

	ifstream iFile("gamestartgenes", ios::in);

	cout << "START Game REAL-TIME EVOLUTION VALIDATION" << endl;

	cout << "Reading in the start genome" << endl;
	//Read in the start Genome
	iFile >> curword;
	iFile >> id;
	cout << "Reading in Genome id -" <<curword<<"- "<< id << endl;
	start_genome = new Genome(id, iFile);
	iFile.close();

	//cout << "Start Genome: " << start_genome <<" "<< start_genome->genome_id <<" "<< start_genome->genes.size() <<" "
		//	<< start_genome->nodes.size()  << endl;

	//Spawn the Population from starter gene
	cout << "Spawning Population off Genome " << NEAT::pop_size << endl;
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

int Game::Game_realtime_loop(Population *pop /*, CartPole *thecart*/) { // return an org with the highest score
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
	int num_species_target = NEAT::pop_size / 5;
    cout <<"****** pop_size: " << pop_size << " num_species_target: " << num_species_target << endl;
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

		if (take_action((*curorg)/*, 1, thecart*/))
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
	for (offspring_count = 0; offspring_count < 2000; offspring_count++) {

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

			cout << "^^^^^^ compat_thresh = " << NEAT::compat_threshold <<" num_species= " <<num_species << endl;
          //  cout << "******************" << endl;
			//Go through entire population, reassigning organisms to new species and print their ID into a file

			for (curorg = (pop->organisms).begin();
					curorg != pop->organisms.end(); ++curorg) {
				pop->reassign_species(*curorg);
			}
		}

		//For printing only
		for (curspec = (pop->species).begin(); curspec != (pop->species).end();
				curspec++) {
			cout << "Species " << (*curspec)->id << " size "
					<< (*curspec)->organisms.size() << " average fitness= "
					<< (*curspec)->average_est << endl;
		}

		cout << "Pop size: " << pop->organisms.size() << endl;

		//Here we call two rtNEAT calls:
		//1) choose_parent_species() decides which species should produce the next offspring
		//2) reproduce_one(...) creates a single offspring from the chosen species
		new_org = (pop->choose_parent_species())->reproduce_one(offspring_count,
				pop, pop->species);

		//Now we evaluate the new individual
		//Note that in a true real-time simulation, evaluation would be happening to all individuals at all times.
		//That is, this call would not appear here in a true online simulation.
		cout << "Evaluating new baby: " << endl;
		if (take_action(new_org/*, 1, thecart*/))
			win = true;

		if (win) {
			cout << "WINNER" << endl;
			pop->print_to_file_by_species("rt_winpop");
			break;
		}

		//Now we reestimate the baby's species' fitness
		new_org->species->estimate_average();

		string outfilename = "src\\in_out\\agentIDs";

		// take action multiple times before removing the worst organism
		for(int i = 0; i < 200; i++) {
			ofstream oFile(outfilename.c_str(), ios::out);
			// print fitness of each agent and take action
			//cout << "Fitness: ";
			for (curorg = (pop->organisms).begin(); curorg != pop->organisms.end(); ++curorg) {
				oFile << (*curorg)->gnome->genome_id << "," << (*curorg)->species->id<<",";
			}
			oFile.close();
			for (curorg = (pop->organisms).begin(); curorg != pop->organisms.end(); ++curorg) {
				take_action(*curorg);
				if((*curorg)->fitness <= -1) {
					(*curorg)->fitness = -1000000;
					pop->remove_worst();
					offspring_count++;
					new_org = (pop->choose_parent_species())->reproduce_one(offspring_count,
									pop, pop->species);
					new_org->species->estimate_average();
				}
				//cout << (*curorg)->fitness << " species: " << (*curorg)->species->id<< "\t";
			}
			//cout << endl;
			Sleep(20);
		}

		//Remove the worst organism
		pop->remove_worst();
        cout << "worst has been removed" << endl;
        cout << "entering the loop" << endl;
		for(int i = 0; i < 100; i++){}
//		std::this_thread::sleep_for (std::chrono::seconds(1));
//		boost::this_thread::sleep( boost::posix_time::milliseconds(1000) );
	}//((*curorg)->gnome)->genome_id=orgcount++;

	double fitn = -1.;
	int high_fit_id = -1;
	for (curorg = (pop->organisms).begin();	curorg != pop->organisms.end(); ++curorg){
		if((*curorg)->fitness > fitn){
               fitn = (*curorg)->fitness;
               high_fit_id = ((*curorg)->gnome)->genome_id;
		}
	}
    cout << "high_fit_id is " << high_fit_id <<endl; // the highest fitness org id
	return high_fit_id;
}

