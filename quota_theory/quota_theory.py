# -*- coding: utf-8 -*-
"""
Created on Wed Feb 12 23:50:30 2025

@author: Dorian
"""

"""
I'm trying ot see what a good quota would be. I'll start off of what Infected Company did
"""

import numpy as np
import matplotlib.pyplot as plt



#%%


def InfectedCompanyQuota(totalLevelValue,Nplayers):
    num2 = totalLevelValue * 0.5
    num3 = max(0, Nplayers - 3) * 0.05
    num4 = totalLevelValue * (0.25 + num3)
    num5 = min(num4, num2)
    return num5

values = np.linspace(0,5000,1000)

plt.figure(figsize=(10,10))
for Nplayers in (6,7,8,9):
    quotas = [InfectedCompanyQuota(val,Nplayers) for val in values]
    
    plt.plot(values,quotas,label = f'{Nplayers} players')
plt.xlim(1,np.max(values))
plt.ylim(1,np.max(values))
# plt.yscale('log')
# plt.xscale("log")
plt.legend()

plt.xlabel('Total level value')
plt.ylabel('Daily Quota')
plt.grid()
plt.show()
