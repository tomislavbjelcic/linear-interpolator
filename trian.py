import numpy as np
from scipy.spatial import Delaunay, ConvexHull
import os
from argparse import ArgumentParser
# for some reason, importing sklearn takes a long time
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
# print("Finished imports")


dlm = ','







def main():



    parser = ArgumentParser()
    parser.add_argument("--datafile", type=str, required=True)
    parser.add_argument("--outfolder", type=str, required=True)
    args = parser.parse_args()

    datafile = args.datafile
    outfolder = args.outfolder


    # stop the program if outfolder exists and it's not a directory
    exists = os.path.exists(outfolder)
    if exists and not os.path.isdir(outfolder):
        print(f"Outfolder {outfolder} exists but it's not a folder.")
        return
        

    if not exists:
        os.makedirs(outfolder)


    
    
        







    # mock data
    # points = np.array([[0, 0], [0, 1.1], [1, 0], [1, 1]])
    # points = np.array([
    #     [0, 0],
    #     [0.7, 0],
    #     [1, 0],
    #     [0.5, 1],
    #     [0.2, -0.4],
    #     [0.9, -0.3]
    # ])
    # z = np.zeros(len(points))

    # load data
    loaded_data = np.loadtxt(fname=datafile, delimiter=dlm)
    points = loaded_data[:,:2]
    z = loaded_data[:,2]



    # compute convex hull and Delaunay triangulation
    hull = ConvexHull(points, incremental=False)
    tri = Delaunay(points, incremental=False)
    print("Computed convex hull and Delaunay triangulation")


    # compute regression coefs, in case query point is outside convex hull
    pf = PolynomialFeatures(degree=2, include_bias=True)
    regression = LinearRegression(fit_intercept=False)
    regression.fit(pf.fit_transform(points), z)
    print(f"Computed regression coefs. {regression.coef_}")


    # save regression coefs
    outfile = os.path.join(outfolder, "coefs.txt")
    with open(outfile, 'w') as f:
        f.write(dlm.join(map(str, regression.coef_)))



    # save points not included in triangulation
    outfile = os.path.join(outfolder, "omitted.txt")
    with open(outfile, 'w') as f:
        f.write(dlm.join(map(str, tri.coplanar[:,0])))



    # save convex hull, counter clockwise
    outfile = os.path.join(outfolder, "convex_hull.txt")
    with open(outfile, 'w') as f:
        f.write(dlm.join(map(str, hull.vertices)))




    # save simplices
    outfile = os.path.join(outfolder, "simplices.txt")
    np.savetxt(fname=outfile, X=tri.simplices, delimiter=dlm, fmt="%d")



    # save plane equations for each simplex
    eqns = []
    for vi in tri.simplices:
        xys = points[vi,:]
        zs = z[vi]
        coefs = np.linalg.solve(np.c_[np.ones(3), xys], zs)
        eqns.append(coefs)

    eqns = np.array(eqns)
    outfile = os.path.join(outfolder, "eqns.txt")
    np.savetxt(fname=outfile, X=eqns, delimiter=dlm, fmt="%f")





    # save neighbors for each simplex
    outfile = os.path.join(outfolder, "neighbors.txt")
    np.savetxt(fname=outfile, X=tri.neighbors, delimiter=dlm, fmt="%d")




    # save simplices for each vertex
    vertex_to_simplices = {}

    for si, vindices in enumerate(tri.simplices):
        for vi in vindices:
            if vi not in vertex_to_simplices:
                vertex_to_simplices[vi] = []
            
            vertex_to_simplices[vi].append(si)

    vts_list = [None for _ in range(len(points))]
    for k,v in vertex_to_simplices.items():
        vts_list[k] = v


    outfile = os.path.join(outfolder, "vertex_to_simplices.txt")
    with open(outfile, 'w') as f:
        for v in vts_list:
            linestr = dlm.join(map(str, v))
            f.write(f"{linestr}\n")


    print("Saved necessary structures")





    # print(tri.simplices)
    # print(tri.neighbors)
    # print(tri.coplanar)
    # print(tri.npoints)
    # print(tri.vertex_to_simplex)
    # print(tri.convex_hull)


    # import matplotlib.pyplot as plt
    # plt.triplot(points[:,0], points[:,1], tri.simplices)
    # plt.plot(points[:,0], points[:,1], 'o')
    # plt.show()






if __name__ == "__main__":
    main()