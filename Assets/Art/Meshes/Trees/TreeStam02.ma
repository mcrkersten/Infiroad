//Maya ASCII 2019 scene
//Name: TreeStam02.ma
//Last modified: Tue, Mar 16, 2021 11:11:21 PM
//Codeset: 1252
requires maya "2019";
currentUnit -l centimeter -a degree -t film;
fileInfo "application" "maya";
fileInfo "product" "Maya 2019";
fileInfo "version" "2019";
fileInfo "cutIdentifier" "201812112215-434d8d9c04";
fileInfo "osv" "Microsoft Windows 10 Technical Preview  (Build 19041)\n";
fileInfo "license" "student";
createNode transform -n "pCylinder4";
	rename -uid "2CAE9A56-42CC-6FEA-0A94-33924F3278CE";
	setAttr ".t" -type "double3" 2.8005767421982268 0 0 ;
	setAttr ".rp" -type "double3" 0.12486811778032947 1.3269244089262096 0 ;
	setAttr ".sp" -type "double3" 0.12486811778032947 1.3269244089262096 0 ;
createNode mesh -n "pCylinder4Shape" -p "pCylinder4";
	rename -uid "1EA4E1A9-4219-A1D0-941D-29AD18F2B9F5";
	setAttr -k off ".v";
	setAttr ".iog[0].og[0].gcl" -type "componentList" 1 "f[0:39]";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".pv" -type "double2" 0.5 0.66855084896087646 ;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr -s 55 ".uvst[0].uvsp[0:54]" -type "float2" 0.53125 0.47716314
		 0.53125 0.40354317 0.5625 0.40351197 0.5625 0.4767797 0.5 0.47695997 0.5 0.40343353
		 0.46875 0.47696 0.46875 0.40343356 0.4375 0.47699237 0.43750003 0.40341946 0.40625
		 0.47716311 0.40624997 0.40354314 0.39028215 0.42572153 0.375 0.40351197 0.625 0.40351197
		 0.625 0.47677967 0.59375 0.47668263 0.59375 0.4035542 0.53125 0.64869714 0.53125
		 0.57971901 0.5625 0.57985741 0.5625 0.64880574 0.5 0.64870286 0.5 0.58015782 0.46875
		 0.64866185 0.46875 0.58027738 0.4375 0.64870286 0.4375 0.58015764 0.40625 0.64869714
		 0.40625 0.57971942 0.375 0.64881033 0.37499997 0.57984406 0.625 0.57984394 0.625
		 0.64881039 0.59375 0.64881039 0.59375006 0.57984304 0.375 0.3125 0.40625 0.3125 0.4375
		 0.3125 0.46875 0.3125 0.5 0.3125 0.53125 0.3125 0.5625 0.3125 0.59375 0.3125 0.625
		 0.3125 0.37500015 0.47677943 0.53125 0.68843985 0.5 0.68843985 0.46875 0.68843985
		 0.4375 0.68843985 0.40625 0.68843985 0.375 0.68843985 0.62499994 0.68843985 0.59375
		 0.68843985 0.5625 0.68843985;
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
	setAttr -s 35 ".pt";
	setAttr ".pt[8]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[9]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[10]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[11]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[12]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[13]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[14]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[15]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[16]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[19]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[21]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[23]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[25]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[27]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[29]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[31]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[32]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[33]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[34]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[35]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[36]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[37]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[38]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[39]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[40]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[41]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[42]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[43]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[44]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[45]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[46]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr ".pt[47]" -type "float3" -1.1920929e-07 0 0 ;
	setAttr -s 48 ".vt[0:47]"  0.32101738 -0.015315354 -0.16899277 0.15202458 -0.015315354 -0.23899187
		 -0.016968178 -0.015315354 -0.16899277 -0.086967267 -0.015315354 0 -0.016968178 -0.015315354 0.16899277
		 0.15202458 -0.015315354 0.23899187 0.32101738 -0.015315354 0.16899279 0.39101651 -0.015315354 0
		 0.16899277 2.34916878 -0.16899277 2.1631055e-24 2.34916878 -0.23899187 -0.16899277 2.34916878 -0.16899277
		 -0.23899187 2.34916878 0 -0.16899277 2.34916878 0.16899277 2.1631055e-24 2.34916878 0.23899187
		 0.16899279 2.34916878 0.16899279 0.23899189 2.34916878 0 0.11396731 1.15215433 0.14136033
		 0.15202458 0.76958019 0.16048485 0.038477838 0.76863521 0.11354675 0.014160743 1.15139854 0.09995681
		 -0.0085548339 0.76863521 0 -0.027242802 1.15139854 0 0.038469259 0.76851392 -0.11355533
		 0.014136809 1.15151906 -0.09995681 0.15202458 0.76958019 -0.16048485 0.11396731 1.15215433 -0.14136033
		 0.26552352 0.76931131 -0.11349893 0.21420777 1.15072787 -0.09995681 0.31249994 0.7696752 0
		 0.25568306 1.1503669 0 0.26552352 0.76931131 0.11349893 0.21420777 1.15072787 0.099956818
		 2.0206201e-09 1.91587734 0.17119113 0.038094781 1.53367937 0.14136033 -0.062186636 1.5353117 0.09995681
		 -0.12105731 1.91594005 0.12105731 -0.10367863 1.53575683 0 -0.17113101 1.91549361 0
		 -0.062186636 1.5353117 -0.09995681 -0.12105731 1.91594005 -0.12105731 0.038094781 1.53367937 -0.14136033
		 2.0206201e-09 1.91587734 -0.1711911 0.13795915 1.5341444 -0.09995681 0.12118702 1.91711259 -0.12118702
		 0.17936267 1.5341444 0 0.17138438 1.91711259 0 0.13794918 1.53419423 0.099956818
		 0.12118146 1.91706192 0.12118146;
	setAttr -s 88 ".ed[0:87]"  0 1 0 1 2 0 2 3 0 3 4 0 4 5 0 5 6 0 6 7 0
		 7 0 0 8 9 0 9 10 0 10 11 0 11 12 0 12 13 0 13 14 0 14 15 0 15 8 0 16 17 0 17 30 0
		 30 31 0 31 16 0 16 19 0 19 18 0 18 17 0 19 21 0 21 20 0 20 18 0 21 23 0 23 22 0 22 20 0
		 23 25 0 25 24 0 24 22 0 25 27 0 27 26 0 26 24 0 27 29 0 29 28 0 28 26 0 29 31 0 30 28 0
		 32 33 0 33 46 0 46 47 0 47 32 0 32 35 0 35 34 0 34 33 0 35 37 0 37 36 0 36 34 0 37 39 0
		 39 38 0 38 36 0 39 41 0 41 40 0 40 38 0 41 43 0 43 42 0 42 40 0 43 45 0 45 44 0 44 42 0
		 45 47 0 46 44 0 1 24 0 26 0 0 2 22 0 3 20 0 4 18 0 5 17 0 6 30 0 7 28 0 16 33 0 34 19 0
		 36 21 0 38 23 0 40 25 0 42 27 0 44 29 0 46 31 0 32 13 0 12 35 0 11 37 0 10 39 0 9 41 0
		 8 43 0 15 45 0 14 47 0;
	setAttr -s 40 -ch 160 ".fc[0:39]" -type "polyFaces" 
		f 4 16 17 18 19
		mu 0 4 0 1 2 3
		f 4 -17 20 21 22
		mu 0 4 1 0 4 5
		f 4 -22 23 24 25
		mu 0 4 5 4 6 7
		f 4 -25 26 27 28
		mu 0 4 7 6 8 9
		f 4 -28 29 30 31
		mu 0 4 9 8 10 11
		f 4 -31 32 33 34
		mu 0 4 11 10 12 13
		f 4 -34 35 36 37
		mu 0 4 14 15 16 17
		f 4 -37 38 -19 39
		mu 0 4 17 16 3 2
		f 4 40 41 42 43
		mu 0 4 18 19 20 21
		f 4 -41 44 45 46
		mu 0 4 19 18 22 23
		f 4 -46 47 48 49
		mu 0 4 23 22 24 25
		f 4 -49 50 51 52
		mu 0 4 25 24 26 27
		f 4 -52 53 54 55
		mu 0 4 27 26 28 29
		f 4 -55 56 57 58
		mu 0 4 29 28 30 31
		f 4 -58 59 60 61
		mu 0 4 32 33 34 35
		f 4 -61 62 -43 63
		mu 0 4 35 34 21 20
		f 4 0 64 -35 65
		mu 0 4 36 37 11 13
		f 4 1 66 -32 -65
		mu 0 4 37 38 9 11
		f 4 2 67 -29 -67
		mu 0 4 38 39 7 9
		f 4 3 68 -26 -68
		mu 0 4 39 40 5 7
		f 4 4 69 -23 -69
		mu 0 4 40 41 1 5
		f 4 5 70 -18 -70
		mu 0 4 41 42 2 1
		f 4 6 71 -40 -71
		mu 0 4 42 43 17 2
		f 4 7 -66 -38 -72
		mu 0 4 43 44 14 17
		f 4 -21 72 -47 73
		mu 0 4 4 0 19 23
		f 4 -24 -74 -50 74
		mu 0 4 6 4 23 25
		f 4 -27 -75 -53 75
		mu 0 4 8 6 25 27
		f 4 -30 -76 -56 76
		mu 0 4 10 8 27 29
		f 4 -33 -77 -59 77
		mu 0 4 45 10 29 31
		f 4 -36 -78 -62 78
		mu 0 4 16 15 32 35
		f 4 -39 -79 -64 79
		mu 0 4 3 16 35 20
		f 4 -20 -80 -42 -73
		mu 0 4 0 3 20 19
		f 4 -45 80 -13 81
		mu 0 4 22 18 46 47
		f 4 -48 -82 -12 82
		mu 0 4 24 22 47 48
		f 4 -51 -83 -11 83
		mu 0 4 26 24 48 49
		f 4 -54 -84 -10 84
		mu 0 4 28 26 49 50
		f 4 -57 -85 -9 85
		mu 0 4 30 28 50 51
		f 4 -60 -86 -16 86
		mu 0 4 34 33 52 53
		f 4 -63 -87 -15 87
		mu 0 4 21 34 53 54
		f 4 -44 -88 -14 -81
		mu 0 4 18 21 54 46;
	setAttr ".cd" -type "dataPolyComponent" Index_Data Edge 0 ;
	setAttr ".cvd" -type "dataPolyComponent" Index_Data Vertex 0 ;
	setAttr ".pd[0]" -type "dataPolyComponent" Index_Data UV 0 ;
	setAttr ".hfd" -type "dataPolyComponent" Index_Data Face 0 ;
createNode groupId -n "groupId2";
	rename -uid "CDB37E1D-4646-0D9C-92E2-B8A22B69B581";
	setAttr ".ihi" 0;
select -ne :time1;
	setAttr ".o" 1;
	setAttr ".unw" 1;
select -ne :hardwareRenderingGlobals;
	setAttr ".otfna" -type "stringArray" 22 "NURBS Curves" "NURBS Surfaces" "Polygons" "Subdiv Surface" "Particles" "Particle Instance" "Fluids" "Strokes" "Image Planes" "UI" "Lights" "Cameras" "Locators" "Joints" "IK Handles" "Deformers" "Motion Trails" "Components" "Hair Systems" "Follicles" "Misc. UI" "Ornaments"  ;
	setAttr ".otfva" -type "Int32Array" 22 0 1 1 1 1 1
		 1 1 1 0 0 0 0 0 0 0 0 0
		 0 0 0 0 ;
	setAttr ".fprt" yes;
select -ne :renderPartition;
	setAttr -s 2 ".st";
select -ne :renderGlobalsList1;
select -ne :defaultShaderList1;
	setAttr -s 4 ".s";
select -ne :postProcessList1;
	setAttr -s 2 ".p";
select -ne :defaultRenderingList1;
select -ne :initialShadingGroup;
	setAttr -s 4 ".dsm";
	setAttr ".ro" yes;
	setAttr -s 4 ".gn";
select -ne :initialParticleSE;
	setAttr ".ro" yes;
select -ne :defaultResolution;
	setAttr ".pa" 1;
select -ne :hardwareRenderGlobals;
	setAttr ".ctrs" 256;
	setAttr ".btrs" 512;
connectAttr "groupId2.id" "pCylinder4Shape.iog.og[0].gid";
connectAttr ":initialShadingGroup.mwc" "pCylinder4Shape.iog.og[0].gco";
connectAttr "pCylinder4Shape.iog.og[0]" ":initialShadingGroup.dsm" -na;
connectAttr "groupId2.msg" ":initialShadingGroup.gn" -na;
// End of TreeStam02.ma
