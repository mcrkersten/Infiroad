//Maya ASCII 2019 scene
//Name: TreeStam.ma
//Last modified: Tue, Mar 16, 2021 04:32:14 PM
//Codeset: 1252
requires maya "2019";
currentUnit -l centimeter -a degree -t film;
fileInfo "application" "maya";
fileInfo "product" "Maya 2019";
fileInfo "version" "2019";
fileInfo "cutIdentifier" "201812112215-434d8d9c04";
fileInfo "osv" "Microsoft Windows 10 Technical Preview  (Build 19041)\n";
fileInfo "license" "student";
createNode transform -n "pCylinder1";
	rename -uid "FF5A8016-4197-3D24-35B5-9CB8718E9100";
	setAttr ".t" -type "double3" 0 0.17642070837945278 0 ;
	setAttr ".s" -type "double3" 0.238991887458987 0.19173605347918724 0.238991887458987 ;
createNode transform -n "transform2" -p "pCylinder1";
	rename -uid "3B69FEA2-4F99-F01B-B5B3-E29CAC262704";
	setAttr ".v" no;
createNode mesh -n "pCylinderShape1" -p "transform2";
	rename -uid "ED1C8D96-429F-5220-7A3B-12B1D7ED90BD";
	setAttr -k off ".v";
	setAttr ".io" yes;
	setAttr -s 2 ".iog[0].og";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".pv" -type "double2" 0.5 0.84375 ;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
createNode transform -n "pCylinder2";
	rename -uid "FE3FB602-43CF-22D4-F152-0581B736B100";
	setAttr ".t" -type "double3" -0.30772008766468961 1.8761673604818569 0 ;
	setAttr ".r" -type "double3" 0 0 50.467989361623978 ;
	setAttr ".s" -type "double3" 0.1320768746621728 0.43166999260110683 0.1320768746621728 ;
createNode transform -n "transform1" -p "pCylinder2";
	rename -uid "0384DBDF-40F9-D485-5925-DCA8D22076CC";
	setAttr ".v" no;
createNode mesh -n "pCylinderShape2" -p "transform1";
	rename -uid "6847C2D0-4FE9-8B6A-DB46-9D85C5B29F85";
	setAttr -k off ".v";
	setAttr ".io" yes;
	setAttr -s 2 ".iog[0].og";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".pv" -type "double2" 0.5 0.56009793281555176 ;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
createNode transform -n "pCylinder3";
	rename -uid "060E17D3-4E81-3FAB-100F-22A3647865B2";
	setAttr ".t" -type "double3" 0.3958186532471919 1.6726916211459537 0 ;
	setAttr ".r" -type "double3" -179.20631373778511 -0.65496007653849619 138.68095913355128 ;
	setAttr ".s" -type "double3" 0.18697272065324713 0.61108739245562738 0.18697272065324713 ;
createNode mesh -n "polySurfaceShape1" -p "pCylinder3";
	rename -uid "5277F931-4FE0-E6B0-460D-C098EC787242";
	setAttr -k off ".v";
	setAttr ".io" yes;
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".pv" -type "double2" 0.5 0.56009793281555176 ;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr -s 56 ".uvst[0].uvsp[0:55]" -type "float2" 0.37883049 0.42886326
		 0.625 0.41761991 0.375 0.57223046 0.62499994 0.54053694 0.62494272 0.56009793 0.375
		 0.3125 0.375 0.41761994 0.40625 0.31250003 0.40624994 0.41770118 0.4375 0.31250006
		 0.43750003 0.41772971 0.46875 0.31250003 0.46875 0.41772661 0.5 0.31250006 0.5 0.41772765
		 0.53125006 0.3125 0.53125006 0.41770115 0.5625 0.3125 0.56250006 0.4176181 0.59375
		 0.3125 0.625 0.3125 0.59375 0.41760981 0.375 0.44833577 0.40624994 0.44849318 0.375
		 0.54053664 0.625 0.44833577 0.59375 0.54053777 0.59375 0.44831902 0.5625 0.54054618
		 0.56250006 0.44833887 0.53125 0.54052168 0.53125006 0.44849318 0.49999997 0.54064173
		 0.50000006 0.44854772 0.46874997 0.54067624 0.46875 0.44854644 0.4375 0.54064173
		 0.43750003 0.44854417 0.40625 0.54052168 0.40625 0.57218397 0.40624994 0.68843985
		 0.375 0.68843979 0.625 0.5722304 0.625 0.68843979 0.59375 0.57223088 0.59375 0.68843979
		 0.5625 0.57222474 0.5625 0.68843979 0.53125 0.57218397 0.53125 0.68843985 0.5 0.57220381
		 0.5 0.68843979 0.46875 0.57219648 0.46874997 0.68843985 0.4375 0.57220381 0.4375
		 0.68843985;
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
	setAttr -s 48 ".vt[0:47]"  0.70710754 -1 -0.70710671 1.9073486e-06 -1 -0.99999988
		 -0.70710564 -1.000000238419 -0.70710671 -0.99999809 -1 0 -0.70710564 -1.000000238419 0.70710671
		 1.9073486e-06 -1 0.99999988 0.70710754 -1 0.70710677 1.000001907349 -1.000000238419 0
		 0.90730667 1.84713006 -0.70710671 0.20020103 1.84713054 -0.99999988 -0.50690556 1.84713006 -0.70710671
		 -0.79979897 1.84713054 0 -0.50690556 1.84713006 0.70710671 0.20020103 1.84713054 0.99999988
		 0.90730667 1.84713006 0.70710677 1.20020008 1.84713006 0 0.36820316 0.17028737 -0.6033771
		 0.3726387 -0.035420895 -0.64142293 0.82982635 -0.036166191 -0.45375046 0.79932976 0.16945815 -0.4266521
		 1.019314766 -0.036258936 0 0.97780037 0.16937041 0 0.82982349 -0.03618288 0.45375484
		 0.79932404 0.16947508 0.42665213 0.3726387 -0.035420895 0.64142293 0.36820316 0.17028737 0.6033771
		 -0.084486961 -0.035178423 0.45349079 -0.062692642 0.17057443 0.4266521 -0.27388096 -0.03518796 0
		 -0.2411232 0.17056775 0 -0.084475517 -0.035159349 -0.45348582 -0.06268692 0.17055535 -0.4266521
		 0.12592125 0.8605268 -0.64072734 0.16205025 0.65487719 -0.6033771 0.59279346 0.65495801 -0.4266521
		 0.5828476 0.86092067 -0.45316416 0.77122211 0.65496182 0 0.7721014 0.86092496 0 0.59277058 0.65500617 0.42665213
		 0.58283138 0.86087203 0.45315161 0.16205025 0.65487719 0.6033771 0.12592125 0.8605268 0.64072734
		 -0.26899338 0.65550923 0.4266521 -0.33090687 0.86069512 0.45310608 -0.44750214 0.65569091 0
		 -0.52011776 0.86063266 0 -0.26899338 0.65550923 -0.4266521 -0.33090687 0.86069512 -0.45310608;
	setAttr -s 88 ".ed[0:87]"  0 1 0 1 2 0 2 3 0 3 4 0 4 5 0 5 6 0 6 7 0
		 7 0 0 8 9 0 9 10 0 10 11 0 11 12 0 12 13 0 13 14 0 14 15 0 15 8 0 16 17 0 17 30 1
		 30 31 0 31 16 1 16 19 1 19 18 0 18 17 1 19 21 1 21 20 0 20 18 1 21 23 1 23 22 0 22 20 1
		 23 25 1 25 24 0 24 22 1 25 27 1 27 26 0 26 24 1 27 29 1 29 28 0 28 26 1 29 31 1 30 28 1
		 32 33 0 33 46 1 46 47 0 47 32 1 32 35 1 35 34 0 34 33 1 35 37 1 37 36 0 36 34 1 37 39 1
		 39 38 0 38 36 1 39 41 1 41 40 0 40 38 1 41 43 1 43 42 0 42 40 1 43 45 1 45 44 0 44 42 1
		 45 47 1 46 44 1 1 17 0 18 0 0 2 30 0 3 28 0 4 26 0 5 24 0 6 22 0 7 20 0 16 33 0 34 19 0
		 36 21 0 38 23 0 40 25 0 42 27 0 44 29 0 46 31 0 32 9 0 8 35 0 15 37 0 14 39 0 13 41 0
		 12 43 0 11 45 0 10 47 0;
	setAttr -s 40 -ch 160 ".fc[0:39]" -type "polyFaces" 
		f 4 16 17 18 19
		mu 0 4 23 8 10 37
		f 4 -17 20 21 22
		mu 0 4 8 23 0 6
		f 4 -22 23 24 25
		mu 0 4 1 25 27 21
		f 4 -25 26 27 28
		mu 0 4 21 27 29 18
		f 4 -28 29 30 31
		mu 0 4 18 29 31 16
		f 4 -31 32 33 34
		mu 0 4 16 31 33 14
		f 4 -34 35 36 37
		mu 0 4 14 33 35 12
		f 4 -37 38 -19 39
		mu 0 4 12 35 37 10
		f 4 40 41 42 43
		mu 0 4 39 38 36 54
		f 4 -41 44 45 46
		mu 0 4 38 39 2 24
		f 4 -46 47 48 49
		mu 0 4 3 4 44 26
		f 4 -49 50 51 52
		mu 0 4 26 44 46 28
		f 4 -52 53 54 55
		mu 0 4 28 46 48 30
		f 4 -55 56 57 58
		mu 0 4 30 48 50 32
		f 4 -58 59 60 61
		mu 0 4 32 50 52 34
		f 4 -61 62 -43 63
		mu 0 4 34 52 54 36
		f 4 0 64 -23 65
		mu 0 4 5 7 8 6
		f 4 1 66 -18 -65
		mu 0 4 7 9 10 8
		f 4 2 67 -40 -67
		mu 0 4 9 11 12 10
		f 4 3 68 -38 -68
		mu 0 4 11 13 14 12
		f 4 4 69 -35 -69
		mu 0 4 13 15 16 14
		f 4 5 70 -32 -70
		mu 0 4 15 17 18 16
		f 4 6 71 -29 -71
		mu 0 4 17 19 21 18
		f 4 7 -66 -26 -72
		mu 0 4 19 20 1 21
		f 4 -21 72 -47 73
		mu 0 4 22 23 38 24
		f 4 -24 -74 -50 74
		mu 0 4 27 25 3 26
		f 4 -27 -75 -53 75
		mu 0 4 29 27 26 28
		f 4 -30 -76 -56 76
		mu 0 4 31 29 28 30
		f 4 -33 -77 -59 77
		mu 0 4 33 31 30 32
		f 4 -36 -78 -62 78
		mu 0 4 35 33 32 34
		f 4 -39 -79 -64 79
		mu 0 4 37 35 34 36
		f 4 -20 -80 -42 -73
		mu 0 4 23 37 36 38
		f 4 -45 80 -9 81
		mu 0 4 2 39 40 41
		f 4 -48 -82 -16 82
		mu 0 4 44 42 43 45
		f 4 -51 -83 -15 83
		mu 0 4 46 44 45 47
		f 4 -54 -84 -14 84
		mu 0 4 48 46 47 49
		f 4 -57 -85 -13 85
		mu 0 4 50 48 49 51
		f 4 -60 -86 -12 86
		mu 0 4 52 50 51 53
		f 4 -63 -87 -11 87
		mu 0 4 54 52 53 55
		f 4 -44 -88 -10 -81
		mu 0 4 39 54 55 40;
	setAttr ".cd" -type "dataPolyComponent" Index_Data Edge 0 ;
	setAttr ".cvd" -type "dataPolyComponent" Index_Data Vertex 0 ;
	setAttr ".pd[0]" -type "dataPolyComponent" Index_Data UV 0 ;
	setAttr ".hfd" -type "dataPolyComponent" Index_Data Face 0 ;
createNode transform -n "transform3" -p "pCylinder3";
	rename -uid "66091BFF-438D-23C9-D049-0B958A4F455E";
	setAttr ".v" no;
createNode mesh -n "pCylinderShape3" -p "transform3";
	rename -uid "55332C89-487B-CA6A-4D50-14A82158E7F2";
	setAttr -k off ".v";
	setAttr ".io" yes;
	setAttr -s 2 ".iog[0].og";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".pv" -type "double2" 0.51562505960464478 0.35404008626937866 ;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
	setAttr -s 5 ".pt";
	setAttr ".pt[40]" -type "float3" -0.00091814261 0.00034039759 0.28017867 ;
	setAttr ".pt[41]" -type "float3" -0.00091814273 0.00034039753 0.080307744 ;
	setAttr ".pt[44]" -type "float3" 0.00097236346 -0.0003604996 -0.2849212 ;
	setAttr ".pt[45]" -type "float3" 0.00097236346 -0.0003604996 -0.0850503 ;
	setAttr ".pt[47]" -type "float3" -0.00091814273 0.00034039753 0.080307744 ;
createNode transform -n "pCylinder4";
	rename -uid "9ED687BC-472F-876E-11DE-74A57243A6CC";
	setAttr ".rp" -type "double3" 0.12486811778032947 1.3269244089262096 0 ;
	setAttr ".sp" -type "double3" 0.12486811778032947 1.3269244089262096 0 ;
createNode mesh -n "pCylinder4Shape" -p "pCylinder4";
	rename -uid "77112B35-40A0-F1F0-AA89-4E8528AB4D80";
	setAttr -k off ".v";
	setAttr -s 2 ".iog[0].og";
	setAttr ".vir" yes;
	setAttr ".vif" yes;
	setAttr ".uvst[0].uvsn" -type "string" "map1";
	setAttr ".cuvs" -type "string" "map1";
	setAttr ".dcc" -type "string" "Ambient+Diffuse";
	setAttr ".covm[0]"  0 1 1;
	setAttr ".cdvm[0]"  0 1 1;
createNode groupParts -n "groupParts4";
	rename -uid "9444ED8F-4B91-0B8B-2379-2CA929A799CE";
	setAttr ".ihi" 0;
	setAttr ".ic" -type "componentList" 1 "f[0:119]";
createNode polyUnite -n "polyUnite1";
	rename -uid "E93B0294-493B-203A-F989-9282BF3643A1";
	setAttr -s 3 ".ip";
	setAttr -s 3 ".im";
createNode groupId -n "groupId2";
	rename -uid "9541C74D-49A7-D801-62E5-A481B70062B7";
	setAttr ".ihi" 0;
createNode groupParts -n "groupParts1";
	rename -uid "77D4614D-406D-DBFA-7D14-4596814415E5";
	setAttr ".ihi" 0;
	setAttr ".ic" -type "componentList" 1 "f[0:39]";
createNode deleteComponent -n "deleteComponent5";
	rename -uid "F7623E99-48C6-997D-54C7-0682C4489A1F";
	setAttr ".dc" -type "componentList" 1 "f[16:23]";
createNode polyTweak -n "polyTweak5";
	rename -uid "D87E4F4A-407C-C096-2447-6D90EE07DB10";
	setAttr ".uopa" yes;
	setAttr -s 9 ".tk";
	setAttr ".tk[48]" -type "float3" -0.0056141978 -0.017624402 0.0019271177 ;
	setAttr ".tk[49]" -type "float3" -0.085787967 0.080752373 -0.022379301 ;
	setAttr ".tk[50]" -type "float3" -0.11596602 0.13181072 -0.033605352 ;
	setAttr ".tk[51]" -type "float3" -0.078107648 0.10565098 -0.025132323 ;
	setAttr ".tk[52]" -type "float3" 0.0052303514 0.017582025 -0.0019674837 ;
	setAttr ".tk[53]" -type "float3" 0.085726291 -0.080766112 0.02237385 ;
	setAttr ".tk[54]" -type "float3" 0.11596601 -0.13181095 0.033605397 ;
	setAttr ".tk[55]" -type "float3" 0.078045398 -0.10566004 0.025126088 ;
createNode polySplitRing -n "polySplitRing5";
	rename -uid "B685AC81-49F6-5541-49A8-F6A000DEBA5B";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 1 "e[64:71]";
	setAttr ".ix" -type "matrix" -0.14041570670321993 0.12344091551388557 0.0021372778495747442 0
		 0.40335986086618481 0.45897383446746581 -0.008464226749162659 0 -0.00331505204326819 -0.00053415974322662324 -0.18694256704077949 0
		 0.3958186532471919 1.6726916211459537 0 1;
	setAttr ".wt" 0.39481320977210999;
	setAttr ".re" 64;
	setAttr ".sma" 29.999999999999996;
	setAttr ".p[0]"  0 0 1;
	setAttr ".fq" yes;
createNode groupId -n "groupId3";
	rename -uid "518CBD75-4F42-CB8B-E717-64B5AA1EB82C";
	setAttr ".ihi" 0;
createNode groupId -n "groupId4";
	rename -uid "5A62B92E-4937-1BE1-3BF9-7EB83B1AC25D";
	setAttr ".ihi" 0;
createNode groupParts -n "groupParts2";
	rename -uid "80C14F37-4B01-370D-DBF5-31A5D2BD2470";
	setAttr ".ihi" 0;
	setAttr ".ic" -type "componentList" 1 "f[0:39]";
createNode polySoftEdge -n "polySoftEdge1";
	rename -uid "1AC47EC1-4107-8DCD-17FB-1781B698C02B";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 16 "e[17]" "e[19:20]" "e[22:23]" "e[25:26]" "e[28:29]" "e[31:32]" "e[34:35]" "e[37:39]" "e[41]" "e[43:44]" "e[46:47]" "e[49:50]" "e[52:53]" "e[55:56]" "e[58:59]" "e[61:63]";
	setAttr ".ix" -type "matrix" 0.238991887458987 0 0 0 0 0.19173605347918724 0 0 0 0 0.238991887458987 0
		 0 0.17642070837945278 0 1;
	setAttr ".a" 0;
createNode deleteComponent -n "deleteComponent2";
	rename -uid "DA997A1E-4D7E-64B8-EF34-67A648244963";
	setAttr ".dc" -type "componentList" 1 "f[0:7]";
createNode deleteComponent -n "deleteComponent1";
	rename -uid "6029A971-47FF-7ABA-FE87-A3973A796ACB";
	setAttr ".dc" -type "componentList" 1 "f[0:7]";
createNode polyBevel3 -n "polyBevel1";
	rename -uid "93964437-4246-D7F3-F3AC-29B3CC80588A";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 14 "e[42]" "e[44]" "e[46]" "e[48]" "e[50]" "e[52]" "e[54:55]" "e[58]" "e[60]" "e[62]" "e[64]" "e[66]" "e[68]" "e[70:71]";
	setAttr ".ix" -type "matrix" 0.1827830084264869 0 0 0 0 0.1827830084264869 0 0 0 0 0.1827830084264869 0
		 0 0.17642070837945278 0 1;
	setAttr ".ws" yes;
	setAttr ".oaf" yes;
	setAttr ".at" 180;
	setAttr ".sn" yes;
	setAttr ".mv" yes;
	setAttr ".mvt" 0.0001;
	setAttr ".sa" 30;
createNode polyTweak -n "polyTweak2";
	rename -uid "35DA28CC-42B3-CBC2-FE09-47A683A7215C";
	setAttr ".uopa" yes;
	setAttr -s 25 ".tk";
	setAttr ".tk[0]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[1]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[2]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[3]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[4]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[5]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[6]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[7]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[16]" -type "float3" 0.63610774 5.5511151e-16 0 ;
	setAttr ".tk[18]" -type "float3" 0.63610774 1.3768338 -0.40851408 ;
	setAttr ".tk[19]" -type "float3" 0.92497087 1.3768338 -0.28886318 ;
	setAttr ".tk[20]" -type "float3" 1.0446216 1.3768338 0 ;
	setAttr ".tk[21]" -type "float3" 0.92497087 1.3768338 0.28886318 ;
	setAttr ".tk[22]" -type "float3" 0.63610774 1.3768338 0.40851408 ;
	setAttr ".tk[23]" -type "float3" 0.3472445 1.3768338 0.28886318 ;
	setAttr ".tk[24]" -type "float3" 0.22759359 1.3768338 0 ;
	setAttr ".tk[25]" -type "float3" 0.34724447 1.3768338 -0.28886321 ;
	setAttr ".tk[26]" -type "float3" 1.2174668e-08 -1.3768337 -0.40851408 ;
	setAttr ".tk[27]" -type "float3" 0.28886321 -1.3768337 -0.28886318 ;
	setAttr ".tk[28]" -type "float3" 0.40851408 -1.3768337 0 ;
	setAttr ".tk[29]" -type "float3" 0.28886321 -1.3768337 0.28886318 ;
	setAttr ".tk[30]" -type "float3" 1.2174668e-08 -1.3768337 0.40851408 ;
	setAttr ".tk[31]" -type "float3" -0.28886318 -1.3768337 0.28886318 ;
	setAttr ".tk[32]" -type "float3" -0.40851408 -1.3768337 0 ;
	setAttr ".tk[33]" -type "float3" -0.28886321 -1.3768337 -0.28886321 ;
createNode polySplitRing -n "polySplitRing2";
	rename -uid "57142AF5-4532-5E97-4FC8-248F8C19C395";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 7 "e[40:41]" "e[43]" "e[45]" "e[47]" "e[49]" "e[51]" "e[53]";
	setAttr ".ix" -type "matrix" 0.1827830084264869 0 0 0 0 0.1827830084264869 0 0 0 0 0.1827830084264869 0
		 0 0.17642070837945278 0 1;
	setAttr ".wt" 0.78216743469238281;
	setAttr ".dr" no;
	setAttr ".re" 40;
	setAttr ".sma" 29.999999999999996;
	setAttr ".p[0]"  0 0 1;
	setAttr ".fq" yes;
createNode polySplitRing -n "polySplitRing1";
	rename -uid "DB893146-4D82-2F8F-FF45-FBBD7A49922B";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 1 "e[16:23]";
	setAttr ".ix" -type "matrix" 0.1827830084264869 0 0 0 0 0.1827830084264869 0 0 0 0 0.1827830084264869 0
		 0 0.17642070837945278 0 1;
	setAttr ".wt" 0.30116918683052063;
	setAttr ".dr" no;
	setAttr ".re" 21;
	setAttr ".sma" 29.999999999999996;
	setAttr ".p[0]"  0 0 1;
	setAttr ".fq" yes;
createNode polyTweak -n "polyTweak1";
	rename -uid "01E3FDB5-471E-48A0-C24B-94BE8F8DCBCF";
	setAttr ".uopa" yes;
	setAttr -s 10 ".tk";
	setAttr ".tk[8]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[9]" -type "float3" 9.0509577e-24 10.331974 0 ;
	setAttr ".tk[10]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[11]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[12]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[13]" -type "float3" 9.0509577e-24 10.331974 0 ;
	setAttr ".tk[14]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[15]" -type "float3" 0 10.331974 0 ;
	setAttr ".tk[17]" -type "float3" 9.0509577e-24 10.331974 0 ;
createNode polyCylinder -n "polyCylinder1";
	rename -uid "52F80045-4240-D1FB-C74B-CDB4C781A716";
	setAttr ".sa" 8;
	setAttr ".sc" 1;
	setAttr ".cuv" 3;
createNode groupId -n "groupId5";
	rename -uid "DFD83164-4EAF-B2C1-177A-4FA58610D70B";
	setAttr ".ihi" 0;
createNode groupId -n "groupId6";
	rename -uid "02E6F9CE-4BED-FEE9-BB4F-B0B0CEA35142";
	setAttr ".ihi" 0;
createNode groupParts -n "groupParts3";
	rename -uid "CC3407F6-474E-CCBB-CCFE-8D8B5FDA4765";
	setAttr ".ihi" 0;
	setAttr ".ic" -type "componentList" 1 "f[0:39]";
createNode polyBevel3 -n "polyBevel2";
	rename -uid "61151F3A-42D3-C947-19F2-22B70C7CD44C";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 14 "e[26]" "e[28]" "e[30]" "e[32]" "e[34]" "e[36]" "e[38:39]" "e[42]" "e[44]" "e[46]" "e[48]" "e[50]" "e[52]" "e[54:55]";
	setAttr ".ix" -type "matrix" 0.084068148566532927 0.10186681116596637 0 0 -0.33293372314256464 0.27476193059932763 0 0
		 0 0 0.1320768746621728 0 -0.30772008766468961 1.8761673604818569 0 1;
	setAttr ".ws" yes;
	setAttr ".oaf" yes;
	setAttr ".f" 0.3;
	setAttr ".at" 180;
	setAttr ".sn" yes;
	setAttr ".mv" yes;
	setAttr ".mvt" 0.0001;
	setAttr ".sa" 30;
createNode polyTweak -n "polyTweak4";
	rename -uid "3691B38E-4C18-70BA-DF04-E4B0B1647811";
	setAttr ".uopa" yes;
	setAttr -s 17 ".tk";
	setAttr ".tk[16]" -type "float3" 0.35020772 0.18566151 0.39662281 ;
	setAttr ".tk[17]" -type "float3" 0.073875867 0.18566151 0.28045461 ;
	setAttr ".tk[18]" -type "float3" -0.040585112 0.18566151 0 ;
	setAttr ".tk[19]" -type "float3" 0.073875807 0.18566151 -0.28045464 ;
	setAttr ".tk[20]" -type "float3" 0.35020772 0.18566151 -0.39662281 ;
	setAttr ".tk[21]" -type "float3" 0.62653977 0.18566151 -0.28045461 ;
	setAttr ".tk[22]" -type "float3" 0.74100035 0.18566151 0 ;
	setAttr ".tk[23]" -type "float3" 0.62653977 0.18566151 0.28045461 ;
	setAttr ".tk[24]" -type "float3" -0.013655582 -0.11719114 0.39662275 ;
	setAttr ".tk[25]" -type "float3" -0.28998768 -0.11719114 0.28045461 ;
	setAttr ".tk[26]" -type "float3" -0.404448 -0.11719114 0 ;
	setAttr ".tk[27]" -type "float3" -0.28998768 -0.11719114 -0.28045464 ;
	setAttr ".tk[28]" -type "float3" -0.013655582 -0.11719114 -0.39662275 ;
	setAttr ".tk[29]" -type "float3" 0.26267663 -0.11719114 -0.28045461 ;
	setAttr ".tk[30]" -type "float3" 0.37713712 -0.11719114 0 ;
	setAttr ".tk[31]" -type "float3" 0.26267663 -0.11719114 0.28045461 ;
createNode polySplitRing -n "polySplitRing4";
	rename -uid "FBE31260-4162-A8D6-08D9-608834E72AE3";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 7 "e[24:25]" "e[27]" "e[29]" "e[31]" "e[33]" "e[35]" "e[37]";
	setAttr ".ix" -type "matrix" 0.084068148566532927 0.10186681116596637 0 0 -0.33293372314256464 0.27476193059932763 0 0
		 0 0 0.1320768746621728 0 -0.30772008766468961 1.8761673604818569 0 1;
	setAttr ".wt" 0.50557214021682739;
	setAttr ".dr" no;
	setAttr ".re" 24;
	setAttr ".sma" 29.999999999999996;
	setAttr ".p[0]"  0 0 1;
	setAttr ".fq" yes;
createNode polySplitRing -n "polySplitRing3";
	rename -uid "5184927D-4E96-D51F-F9A1-FCB3DA32CBF1";
	setAttr ".uopa" yes;
	setAttr ".ics" -type "componentList" 1 "e[16:23]";
	setAttr ".ix" -type "matrix" 0.084068148566532927 0.10186681116596637 0 0 -0.33293372314256464 0.27476193059932763 0 0
		 0 0 0.1320768746621728 0 -0.30772008766468961 1.8761673604818569 0 1;
	setAttr ".wt" 0.3095262348651886;
	setAttr ".re" 17;
	setAttr ".sma" 29.999999999999996;
	setAttr ".p[0]"  0 0 1;
	setAttr ".fq" yes;
createNode polyTweak -n "polyTweak3";
	rename -uid "B0B024FD-4429-4CFD-E0A5-C3999AF58813";
	setAttr ".uopa" yes;
	setAttr -s 8 ".tk[8:15]" -type "float3"  0.20019993 0.84713036 0 0.20019993
		 0.84713036 0 0.20019993 0.84713036 0 0.20019993 0.84713036 0 0.20019993 0.84713036
		 0 0.20019993 0.84713036 0 0.20019993 0.84713036 0 0.20019993 0.84713036 0;
createNode deleteComponent -n "deleteComponent4";
	rename -uid "30DB289C-4637-577B-0796-21A565F3E161";
	setAttr ".dc" -type "componentList" 1 "f[8:15]";
createNode deleteComponent -n "deleteComponent3";
	rename -uid "4A4B8DC4-4D26-7D20-C01F-31AD76BE7D87";
	setAttr ".dc" -type "componentList" 1 "f[16:23]";
createNode polyCylinder -n "polyCylinder2";
	rename -uid "D90D44F6-42B2-18B7-5241-AC9EFBBDA5D9";
	setAttr ".sa" 8;
	setAttr ".sc" 1;
	setAttr ".cuv" 3;
createNode groupId -n "groupId7";
	rename -uid "62BD9B7C-4B20-125D-D9A0-AF9F05C7A7A1";
	setAttr ".ihi" 0;
createNode groupId -n "groupId8";
	rename -uid "1A8C98D1-40B1-1588-3D47-A4A1F0377D9C";
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
	setAttr -s 8 ".dsm";
	setAttr ".ro" yes;
	setAttr -s 8 ".gn";
select -ne :initialParticleSE;
	setAttr ".ro" yes;
select -ne :defaultRenderGlobals;
	setAttr ".ren" -type "string" "arnold";
select -ne :defaultResolution;
	setAttr ".pa" 1;
select -ne :hardwareRenderGlobals;
	setAttr ".ctrs" 256;
	setAttr ".btrs" 512;
select -ne :ikSystem;
	setAttr -s 4 ".sol";
connectAttr "groupId4.id" "pCylinderShape1.iog.og[0].gid";
connectAttr ":initialShadingGroup.mwc" "pCylinderShape1.iog.og[0].gco";
connectAttr "groupParts2.og" "pCylinderShape1.i";
connectAttr "groupId5.id" "pCylinderShape1.ciog.cog[0].cgid";
connectAttr "groupId6.id" "pCylinderShape2.iog.og[0].gid";
connectAttr ":initialShadingGroup.mwc" "pCylinderShape2.iog.og[0].gco";
connectAttr "groupParts3.og" "pCylinderShape2.i";
connectAttr "groupId7.id" "pCylinderShape2.ciog.cog[0].cgid";
connectAttr "groupId2.id" "pCylinderShape3.iog.og[0].gid";
connectAttr ":initialShadingGroup.mwc" "pCylinderShape3.iog.og[0].gco";
connectAttr "groupParts1.og" "pCylinderShape3.i";
connectAttr "groupId3.id" "pCylinderShape3.ciog.cog[0].cgid";
connectAttr "groupParts4.og" "pCylinder4Shape.i";
connectAttr "groupId8.id" "pCylinder4Shape.iog.og[0].gid";
connectAttr ":initialShadingGroup.mwc" "pCylinder4Shape.iog.og[0].gco";
connectAttr "polyUnite1.out" "groupParts4.ig";
connectAttr "groupId8.id" "groupParts4.gi";
connectAttr "pCylinderShape3.o" "polyUnite1.ip[0]";
connectAttr "pCylinderShape1.o" "polyUnite1.ip[1]";
connectAttr "pCylinderShape2.o" "polyUnite1.ip[2]";
connectAttr "pCylinderShape3.wm" "polyUnite1.im[0]";
connectAttr "pCylinderShape1.wm" "polyUnite1.im[1]";
connectAttr "pCylinderShape2.wm" "polyUnite1.im[2]";
connectAttr "deleteComponent5.og" "groupParts1.ig";
connectAttr "groupId2.id" "groupParts1.gi";
connectAttr "polyTweak5.out" "deleteComponent5.ig";
connectAttr "polySplitRing5.out" "polyTweak5.ip";
connectAttr "polySurfaceShape1.o" "polySplitRing5.ip";
connectAttr "pCylinderShape3.wm" "polySplitRing5.mp";
connectAttr "polySoftEdge1.out" "groupParts2.ig";
connectAttr "groupId4.id" "groupParts2.gi";
connectAttr "deleteComponent2.og" "polySoftEdge1.ip";
connectAttr "pCylinderShape1.wm" "polySoftEdge1.mp";
connectAttr "deleteComponent1.og" "deleteComponent2.ig";
connectAttr "polyBevel1.out" "deleteComponent1.ig";
connectAttr "polyTweak2.out" "polyBevel1.ip";
connectAttr "pCylinderShape1.wm" "polyBevel1.mp";
connectAttr "polySplitRing2.out" "polyTweak2.ip";
connectAttr "polySplitRing1.out" "polySplitRing2.ip";
connectAttr "pCylinderShape1.wm" "polySplitRing2.mp";
connectAttr "polyTweak1.out" "polySplitRing1.ip";
connectAttr "pCylinderShape1.wm" "polySplitRing1.mp";
connectAttr "polyCylinder1.out" "polyTweak1.ip";
connectAttr "polyBevel2.out" "groupParts3.ig";
connectAttr "groupId6.id" "groupParts3.gi";
connectAttr "polyTweak4.out" "polyBevel2.ip";
connectAttr "pCylinderShape2.wm" "polyBevel2.mp";
connectAttr "polySplitRing4.out" "polyTweak4.ip";
connectAttr "polySplitRing3.out" "polySplitRing4.ip";
connectAttr "pCylinderShape2.wm" "polySplitRing4.mp";
connectAttr "polyTweak3.out" "polySplitRing3.ip";
connectAttr "pCylinderShape2.wm" "polySplitRing3.mp";
connectAttr "deleteComponent4.og" "polyTweak3.ip";
connectAttr "deleteComponent3.og" "deleteComponent4.ig";
connectAttr "polyCylinder2.out" "deleteComponent3.ig";
connectAttr "pCylinderShape3.iog.og[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinderShape3.ciog.cog[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinderShape1.iog.og[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinderShape1.ciog.cog[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinderShape2.iog.og[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinderShape2.ciog.cog[0]" ":initialShadingGroup.dsm" -na;
connectAttr "pCylinder4Shape.iog.og[0]" ":initialShadingGroup.dsm" -na;
connectAttr "groupId2.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId3.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId4.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId5.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId6.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId7.msg" ":initialShadingGroup.gn" -na;
connectAttr "groupId8.msg" ":initialShadingGroup.gn" -na;
// End of TreeStam.ma
