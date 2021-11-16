import sys
import numpy as np
import math
import matplotlib.pyplot as plt
from numpy.lib.polynomial import poly
import pandas as pd
from matplotlib import cm
import matplotlib.patheffects as path_effects
import random

class Run:
    def __init__(self, run_name, linearities, leniencies, ids):
        self.run_name = run_name
        self.linearities = linearities
        self.leniencies = leniencies
        self.ids = ids
        self.sample_size = len(linearities)

        self.PerformCalculations()

    def PerformCalculations(self):
        self.min_leniency = min(self.leniencies)
        self.max_leniency = max(self.leniencies)

        self.normalized_leniencies = list(map(lambda x:  self.normalize(x, self.min_leniency, self.max_leniency), self.leniencies))

        self.min_linearity = min(self.linearities)
        self.max_linearity = max(self.linearities)

        self.normalized_linearities = list(map(lambda x: self.normalize(x, self.min_linearity, self.max_linearity), self.linearities))

        self.average_linearity = sum(self.linearities)/len(self.linearities)
        self.average_leniency = sum(self.leniencies)/len(self.leniencies)

        self.average_point = (self.average_linearity, self.average_leniency)
        self.normalized_average_point = (self.normalize(self.average_linearity, self.min_linearity, self.max_linearity), self.normalize(self.average_leniency,  self.min_leniency, self.max_leniency))

        self.std_linearity = np.std(self.linearities)
        self.std_leniency = np.std(self.leniencies)

    def normalize(self, x, min, max):
        return (x - min) / (max - min)

    def __str__(self):
        return f'<name: {self.run_name}, sample_size: {len(self.ids)}, average linearity: {round(self.average_linearity, 2)}, average leniency: {round(self.average_leniency, 2)}>'

    def __repr__(self):
        return str(self)

def print_random_samples_of_maps(run, num):
    random_sample = random.sample(run.ids, num)
    for sample in random_sample:
        print((sample, run.normalized_linearities[sample], run.normalized_leniencies[sample],  run.linearities[sample], run.leniencies[sample]))

def find_same_normalized_data_point(id_list1, id_list2, normalized_linearity, normalized_leniency, linearity_weight):
    firstLevel = min(id_list1, key=lambda entry:abs(entry[1]-normalized_linearity)*linearity_weight+abs(entry[2]-normalized_leniency))
    secondLevel = min(id_list2, key=lambda entry:abs(entry[1]-normalized_linearity)*linearity_weight+abs(entry[2]-normalized_leniency))
    return (firstLevel, secondLevel)

filename = f'example_generator_config.txt'

run_names = []
number_of_maps_to_randomly_sample = 0

with open(filename) as f:
    name_of_everything = f.readline().rstrip('\n')
    number_of_maps_to_randomly_sample = int(f.readline())
    run_name = f.readline()
    while run_name != "":
        run_names.append(run_name.rstrip('\n'))
        run_name = f.readline()
    print(run_names)    

number_of_runs = len(run_names)
runs = []

for i in range(len(run_names)):
    run_name = run_names[i]
    run_file_name = f'{run_name}/data.txt'

    with open(run_file_name) as f:
        display_name = f.readline()
        ids = []
        leniencies = []
        linearities = []
        id = f.readline()
        while id != "":
            ids.append(int(id))
            leniency = f.readline().rstrip('\n').replace(',',".")
            leniencies.append(float(leniency))
            linearity = f.readline().rstrip('\n').replace(',',".")
            linearities.append(float(linearity))
            id = f.readline()

        runs.append(Run(display_name, linearities, leniencies, ids))

normalized_id_lists = []

for run in runs:
    normalized_id_lists.append(list(zip(run.ids, run.normalized_linearities, run.normalized_leniencies, run.linearities, run.leniencies)))


print_random_samples_of_maps(runs[0], number_of_maps_to_randomly_sample)

print("Enter normalized linearity")
normalized_linearity = float(input())
print("Enter normalized leniency")
normalized_leniency = float(input())
print("How weighted should linearity be?")
linearity_weight = float(input())
print("Which runs to get it from")
print("Run nmbr 1")
run_number_1 = int(input())
print("Run nmbr 2")
run_number_2 = int(input())

id_list1 = normalized_id_lists[run_number_1]
id_list2 = normalized_id_lists[run_number_2]

print(find_same_normalized_data_point(id_list1, id_list2, normalized_linearity, normalized_leniency, linearity_weight))


