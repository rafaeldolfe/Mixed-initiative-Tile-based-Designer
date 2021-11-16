import sys
import numpy as np
import math
import matplotlib.pyplot as plt
from numpy.lib.polynomial import poly
import pandas as pd
from matplotlib import cm
import matplotlib.patheffects as path_effects

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

def set_chart(xarray, yarray, point, axis, vmin, vmax, grid_size, extent):
    x = np.array(xarray)
    y = np.array(yarray)

    print("INFO INSIDE SET_CHART")
    print(grid_size)
    print(vmin)
    print(vmax)
    print(extent)
    hb = axis.hexbin(x, y, gridsize=int(grid_size), cmap=cm.get_cmap('RdYlBu_r'), vmin=vmin, vmax=vmax, extent=extent)
    axis.scatter(
            point[0], 
            point[1], s=60, c="white", marker="o", edgecolors="black", label="Average Point"
        )
    axis.set_facecolor('#313695')
    axis.set_title(run.run_name)

    return hb

def get_params_norm(run):
    return (run.normalized_linearities, run.normalized_leniencies, run.normalized_average_point, None)

def get_params(run):
    extent = [absolute_min_linearity, absolute_max_linearity, absolute_min_leniency, absolute_max_leniency]
    return (run.linearities, run.leniencies, run.average_point, extent)

if len(sys.argv) > 1:
    exit()

filename = f'config.txt'

run_names = []

with open(filename) as f:
    name_of_everything = f.readline().rstrip('\n')
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

row_number = math.ceil(math.sqrt(number_of_runs))
row_number = 3
col_number = 4

fig, axes = plt.subplots(nrows=row_number, ncols=col_number)

print(type(axes))

if not isinstance(axes, np.ndarray):
    axes = [[axes]]


grid_size = 33

absolute_max_hexagon_value = 0
absolute_max_linearity = float("-inf")
absolute_min_linearity = float("inf")
absolute_max_leniency = float("-inf")
absolute_min_leniency = float("inf")
counter = 0
try:
    for row_axes in axes:
        for axis in row_axes:
            if (counter >= len(runs)):
                break
            run = runs[counter]

            if run.max_linearity > absolute_max_linearity:
                absolute_max_linearity = run.max_linearity
            if run.min_linearity < absolute_min_linearity:    
                absolute_min_linearity = run.min_linearity
            if run.max_leniency > absolute_max_leniency:
                absolute_max_leniency = run.max_leniency
            if run.min_leniency < absolute_min_leniency:
                absolute_min_leniency = run.min_leniency

            poly_collection = set_chart(run.linearities, run.leniencies, run.average_point, axis, 0, None, grid_size, None)
            array = poly_collection.get_array()
            max_hexagon_value = max(array)
            
            if max_hexagon_value > absolute_max_hexagon_value:
                absolute_max_hexagon_value = max_hexagon_value

            counter+=1
except:
    axis = axes

    run = runs[0]

    if run.max_linearity > absolute_max_linearity:
        absolute_max_linearity = run.max_linearity
    if run.min_linearity < absolute_min_linearity:    
        absolute_min_linearity = run.min_linearity
    if run.max_leniency > absolute_max_leniency:
        absolute_max_leniency = run.max_leniency
    if run.min_leniency < absolute_min_leniency:
        absolute_min_leniency = run.min_leniency

    poly_collection = set_chart(run.linearities, run.leniencies, run.average_point, axis, 0, None, grid_size, None)
    array = poly_collection.get_array()
    max_hexagon_value = max(array)
    
    if max_hexagon_value > absolute_max_hexagon_value:
        absolute_max_hexagon_value = max_hexagon_value

plt.close()

visualization_dict_info = {0: get_params_norm, 1: get_params}
final_rect = (0.1,0.05,0.8,0.95)
final_figsize = (9.9*1.45,8.8)
fig, axes = plt.subplots(nrows=row_number, ncols=col_number, sharex=True, sharey=True, figsize=final_figsize)
hb = None

if not isinstance(axes, np.ndarray):
    axes = [[axes]]

for row_axes in axes:
    for axis in row_axes:
        axis.set_visible(False)

counter = 0
for row_axes in axes:
    for axis in row_axes:
        if (counter >= len(runs)):
            break
        run = runs[counter]

        parameters = visualization_dict_info[0](run)

        hb = set_chart(parameters[0], parameters[1], parameters[2], axis, 0, absolute_max_hexagon_value, grid_size, parameters[3])
        axis.set_visible(True)
        counter+=1

fig.subplots_adjust(right=0.8)
cbar_ax = fig.add_axes([0.85, 0.15, 0.025, 0.7])
cb = fig.colorbar(hb, cax=cbar_ax)
cb.set_label('counts')

for row_axes in axes:
    for axis in row_axes:
        handles, labels = axis.get_legend_handles_labels()
        break
    break

fig.suptitle('Normalized data', fontsize=20)
fig.add_subplot(111, frameon=False)
plt.tick_params(labelcolor='none', which='both', top=False, bottom=False, left=False, right=False)
plt.xlabel("linearity", labelpad=25, size=16)
plt.ylabel("leniency", labelpad=25, size=16)
fig.legend(handles, labels, loc = 'lower center')
fig.tight_layout(rect=final_rect)

plt.savefig(f"Analyses/{name_of_everything}-normalized.png")

plt.close()

fig, axes = plt.subplots(nrows=row_number, ncols=col_number, sharex=True, sharey=True, figsize=final_figsize)

if not isinstance(axes, np.ndarray):
    axes = [[axes]]

for row_axes in axes:
    for axis in row_axes:
        axis.set_visible(False)

counter = 0
for row_axes in axes:
    for axis in row_axes:
        if (counter >= len(runs)):
            break
        run = runs[counter]

        parameters = visualization_dict_info[1](run)

        hb = set_chart(parameters[0], parameters[1], parameters[2], axis, 0, absolute_max_hexagon_value, grid_size, parameters[3])
        axis.axvline(x=run.min_linearity, label='bounds of linearity', color="yellow")
        axis.axvline(x=run.max_linearity, color="yellow")
        axis.axhline(y=run.min_leniency, label='bounds of leniency', color="orange")
        axis.axhline(y=run.max_leniency, color="orange")
        axis.set_visible(True)
        counter+=1

fig.subplots_adjust(right=0.8)
cbar_ax = fig.add_axes([0.85, 0.15, 0.025, 0.7])
cb = fig.colorbar(hb, cax=cbar_ax)
cb.set_label('counts')

for row_axes in axes:
    for axis in row_axes:
        handles, labels = axis.get_legend_handles_labels()
        break
    break

fig.suptitle('Raw data', fontsize=20)
fig.add_subplot(111, frameon=False)
plt.tick_params(labelcolor='none', which='both', top=False, bottom=False, left=False, right=False)
plt.xlabel("linearity", labelpad=25, size=16)
plt.ylabel("leniency", labelpad=25, size=16)
fig.legend(handles, labels, loc = 'lower center')
fig.tight_layout(rect=final_rect)

plt.savefig(f"Analyses/{name_of_everything}.png")