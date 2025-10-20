P = [0 0 0;  1 2 1;  3 3 0];
p = 2;
% length(U) must be N+p+1 = 5+2+1 = 8
U = [0 0 0  0.3 0.6  1 1 1];
[C, U] = bspline_plot(P, p, 20, U);