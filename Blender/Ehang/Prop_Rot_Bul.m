clear all
close all
clc

%% Kullanici Girdileri
% Blender icin X Ileri Y Sola Z Asagi
blender.prop1.head = [1.2332,1.219,-0.206];
blender.prop1.tail = [1.2048,1.1914,0.18695];
blender.prop2.head = [1.223,-1.2266,-0.206];
blender.prop2.tail = [1.1948,-1.1988,0.18695];
blender.prop3.head = [-1.184,-1.2386,-0.20853];
blender.prop3.tail = [-1.1733,-1.202,0.18457];
blender.prop4.head = [-1.1736,1.2511,-0.20853];
blender.prop4.tail = [-1.1632,1.2144,0.18457];

%% Hesaplar
fn = fieldnames(blender);

for i = 1:length(fn)
secilenProp                         = fn{i};
blender.(secilenProp).vector        = blender.(secilenProp).tail - blender.(secilenProp).head;
blender.(secilenProp).vector_length = sqrt(dot(blender.(secilenProp).vector,blender.(secilenProp).vector'));
blender.(secilenProp).unit_vector   = blender.(secilenProp).vector ./blender.(secilenProp).vector_length;
blender.(secilenProp).costF = @(Z)cost(Z,blender.(secilenProp).unit_vector);
x = lsqnonlin(blender.(secilenProp).costF,[0,0,0]);
blender.(secilenProp).costVal = blender.(secilenProp).costF(x);
disp(blender.(secilenProp).costVal)
blender.(secilenProp).acilar_deg = rad2deg(x);

end





% UE5 icin X Ileri Y Saga Z Yukari rotation order ZYX