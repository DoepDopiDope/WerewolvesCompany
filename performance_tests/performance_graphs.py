# -*- coding: utf-8 -*-
"""
Created on Sun Jan 26 11:33:48 2025

@author: Dorian
"""

import numpy as np
import matplotlib.pyplot as plt
import os
import pandas as pd
from datetime import datetime

#%%

basedir = "G:/Custom Games/Modding/LethalCompanyMods/WerewolvesCompany/performance_tests/"

os.chdir(basedir)
alldirs = []

for dire in os.listdir():
    if not os.path.isdir(dire):
        continue
    alldirs.append(dire)

alldirs = ['0.1.6','0.1.6.2_no_objects_calls','0.2.0','0.2.0_big_bundle','vanilla']
# alldirs = ["0.2.0", "0.2.0_big_bundle","vanilla"]
alldirs = ["0.5.0","0.4.0","vanilla"]


def getCycler(axes=None):
    if axes is None:
        axes = plt.gca()
    return axes._get_lines.prop_cycler

fig,axes = plt.subplots(1,1, figsize = (20,5))
axes = [axes]

for dire in alldirs:
    os.chdir(f'{basedir}/{dire}')
    for file in os.listdir():
        if file.startswith('FPS'):
            break
    print(file)
    df = pd.read_csv(file)
    

    
    fps99p = df['99th% FPS']
    time = df['TIME STAMP']
    
    
    time = np.array([datetime.fromisoformat(t) for t in time.values.astype(str)])
    time = (time - time[0])
    time = np.array([t.total_seconds() for t in time])
    
    types = ('FPS', '99th% FPS')
    color = next_color = axes[0]._get_lines.get_next_color()
    lsts = ('-', '--')
    if dire == 'vanilla':
        color = 'k'
    for typ,lst in zip(types,lsts):
        fps = df[typ]
        axes[0].plot(time,fps, label = f'{dire} - {typ}',color=color,linestyle=lst)

axes[0].set_xlabel('Elapsed time (s)')
# axes[1].set_xlabel('Elapsed time (s)')
axes[0].set_ylabel('FPS')
# axes[1].set_ylabel('99% FPS')

for ax in axes:
    ax.legend(loc = 'upper right')
plt.savefig(f'{basedir}/performance_0.5.0.png', dpi=300, bbox_inches = 'tight')
plt.show()

