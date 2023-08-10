import numpy as np
from numpy.fft import *
from matplotlib import cm, colors
import sys
import os

def gen_kgrid():
    dk = 2*np.pi/boxsize
    ks = fftfreq(N)*N*dk
    kx = ks.reshape(-1,1,1)
    ky = ks.reshape(1,-1,1)
    ks = rfftfreq(N)*N*dk
    kz = ks.reshape(1,1,-1)
    return kx, ky, kz

def gen_xgrid():
    tmp = np.linspace(0, N-1, N)
    # tmp[N//2:] -= N
    tmp *= boxsize/N
    x = tmp.reshape(-1,1,1)
    y = tmp.reshape(1,-1,1)
    z = tmp.reshape(1,1,-1)
    return x, y, z

def adj_complex_conj(arr_, set_DC_zero=True):
    '''
    We need to adjust some values of arr to make sure the inverse DFT of arr gives a real array.
    I will only deal with rfft for now, so make sure the shape of arr is correct.
    '''
    arr = arr_.copy()

    ## I am basically copying from 21cmFAST
    dim = arr.shape[0]
    middle = dim//2

    # corners
    for i in [0, middle]:
        for j in [0, middle]:
            for k in [0, middle]:
                arr[i,j,k] = np.real(arr[i,j,k])
    # set the DC mode to 0
    if set_DC_zero:
        arr[0,0,0] = 0

    ind = np.arange(1, middle, 1, dtype=int)
    # do all of i except corners
    # just j corners
    for j in [0, middle]:
        for k in [0, middle]:
            arr[ind,j,k] = np.conjugate(arr[dim-ind,j,k])
    # all of j
    for j in range(1, middle):
        for k in [0, middle]:
            arr[ind,j,k] = np.conjugate(arr[dim-ind,dim-j,k])
            arr[ind,dim-j,k] = np.conjugate(arr[dim-ind,j,k])

    return arr

def gen_deltak_Pk():
    '''
    Given a power spectrum, generate a random realization of the source grid.
    L is the box size. N is the number of grid points on a size.
    Pk is the power spectrum function that takes the wavenumber and returns the power.
    Let's use the same Fourier convention as Mesinger & Furlanetto 07.
    '''
    middle = N//2
    volume = boxsize**3

    kx, ky, kz = gen_kgrid()
    kn = (kx**2 + ky**2 + kz**2)**0.5

    # create k space Gaussian random field based on Pk
    # generate random numbers
    a = np.random.randn(N,N,middle+1)
    b = np.random.randn(N,N,middle+1)
    rhok = (volume * Pk_init(kn) / 2)**0.5 * (a + b*1j)
    
    return adj_complex_conj(rhok)

def calc_ZA_displacement(deltak_1):
    Nh = N//2
    kx, ky, kz = gen_kgrid()
    k2 = kx**2 + ky**2 + kz**2
    k2[0,0,0] = 1e-6
    psi1x = deltak_1*0.
    psi1y = deltak_1*0.
    psi1z = deltak_1*0.

    tmp = deltak_1/k2*(-1)**0.5
    psi1x = tmp*kx
    psi1y = tmp*ky
    psi1z = tmp*kz

    fac = N**3/boxsize**3
    psi1x = irfftn(psi1x)*fac
    psi1y = irfftn(psi1y)*fac
    psi1z = irfftn(psi1z)*fac

    return psi1x, psi1y, psi1z

def calc_pos(psi):
    psi1x, psi1y, psi1z = psi
    x, y, z = gen_xgrid()
    pos = np.zeros((N**3,3))
    pos[:,0] = (x + psi1x).ravel()
    pos[:,1] = (y + psi1y).ravel()
    pos[:,2] = (z + psi1z).ravel()
    pos[pos > boxsize] -= boxsize
    pos[pos < 0.] += boxsize
    return pos

def Pk_init(ks, kb=1, slope=-1):
    Pks = ks**-1
    ii = ks > kb
    Pks[ii] = kb**slope
    return Pks

boxsize = 1000

N = int(sys.argv[1])
kb = float(sys.argv[2])
slope = float(sys.argv[3])
seed = int(sys.argv[4])
outpath = sys.argv[-1]

# initial power spectrum
def Pk_init(ks):
    Pks = ks**-1
    ii = ks > kb
    Pks[ii] = kb**-1 * (ks[ii]/kb)**slope
    return Pks

np.random.seed(seed)

# generate deltak_1 and delta1 fields
deltak_1 = gen_deltak_Pk()
delta1 = irfftn(deltak_1)
delta1 /= np.std(delta1) # normalize
deltak_1 = rfftn(delta1)*boxsize**3/N**3

# calculate final position... factor of 5 is artificial and fine-tuned to make things look good
psi = calc_ZA_displacement(deltak_1*5)
pos = calc_pos(psi)

# prepare to dump
ii = np.random.choice(len(pos), 12**3, replace=False)
data = np.zeros((len(ii), 6))
data[:,:3] = pos[ii]/boxsize # amplitude normalize to 1
norm = colors.Normalize(-2, 2)
m = cm.ScalarMappable(norm=norm, cmap='YlOrRd')
data[:,3:] = m.to_rgba(delta1.flatten()[ii])[:,:3]

np.savetxt(outpath + 'LSSModel.txt', data)
print('script run')

