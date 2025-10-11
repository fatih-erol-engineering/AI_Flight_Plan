function costVal = cost(Z,unitVector)
V1 = [0,0,1];
dcmMat = angle2dcm(Z(3),Z(2),Z(1),'ZYX');

V2 = dcmMat*V1';
costVal = abs(unitVector - V2');

end

