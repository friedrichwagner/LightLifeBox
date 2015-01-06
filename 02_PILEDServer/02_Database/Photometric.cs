using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumitech.Helpers
{

    public enum Observer { Observer2deg = 0, Observer10deg = 1 };

    public struct CIExy
    {
        public double x;
        public double y;
    }

    public struct CIECoords
    {
        public double x;
        public double y;
        public double z;
    }

    public struct Tristimulus
    {
        public double X;
        public double Y;
        public double Z;
        public Tristimulus(double x, double y, double z)
        {
            X = x; Y = y; Z = z;
        }
    }

   /* public struct cmCIECMF
    {
        int wl; //in nm
        Tristimulus cmf;
        Observer observer;
        public cmCIECMF(int l, Tristimulus t2, Observer o)
        {
            wl = l; cmf = t2; observer = t10;
        }
    }*/

    public struct CRI
    {
        public double CommonCRI;
        public double CRI1;
        public double CRI2;
        public double CRI3;
        public double CRI4;
        public double CRI5;
        public double CRI6;
        public double CRI7;
        public double CRI8;
        public double CRI9;
        public double CRI10;
        public double CRI11;
        public double CRI12;
        public double CRI13;
        public double CRI14;
        public double CRI15;
        public double CRI16;
    }

    public struct MacAdams
    {
        public int McAId { get; set; } //Muss Property sein (nicht Feld), damit man es in Combobox anzeigen kann mit Displaymember/Valuemember
        public string MCAName {get;set;}
        public double x0;
        public double y0;
        public double CCT0;
        public int g11;
        public int g12;
        public int g22;
        public Observer observer;
        public MacAdams(int ID, string Name, double x, double y, double cct, int g1, int g2, int g3, Observer o):this()
        {
            x0 = x; y0 = y; CCT0 = cct; observer = 0; g11 = g1; g12 = g2; g22 = g3;
            McAId=ID; MCAName=Name; 
        }
    }

    public struct PolygonBin
    {
	    public string	BinName;
	    public int		npoints;
	    public CIExy[]	Polygon;
        public PolygonBin(string name, int anzPoints)
        {
            BinName = name; npoints = anzPoints;
            Polygon = new CIExy[npoints];
        }
    }

    public static class Photometric
    {
        public static  List<MacAdams> listMacAdam = new List<MacAdams>();
        public static List<PolygonBin> listBins = new List<PolygonBin>();
        private static LTSQLCommand cmd;
        private static bool isInitialized = false;

        private const double pi	=3.14159265358979;
        private const double licht_c=299792458; // ms^-2
        private const double boltzmann_k=1.3806505e-23; // JK^-1
        private const double planck_h=6.6260693e-34; //Js
        private const double ACCURACY=0.000001;

        private static Dictionary<int, Tristimulus> CIEColorMatchingFunctions2 = new Dictionary<int,Tristimulus>();
        private static Dictionary<int, Tristimulus> CIEColorMatchingFunctions10 = new Dictionary<int, Tristimulus>();
        private const int cmfCount = 471;



        public static void Init(LTSQLCommand pCmd)
        {
            cmd = pCmd;

            if (pCmd != null)
            {
                loadMacAdamListfromDB();
                loadBinListfromDB();
            }
            else
            {
                loadMacAdamListfromSource();
                loadBinListfromSource();
            }

            InitCMF();

            if ((CIEColorMatchingFunctions2.Count() != cmfCount) || (CIEColorMatchingFunctions10.Count() != cmfCount))
            {
                throw new ArgumentOutOfRangeException("Color Matching Function size not 471");
            }

            isInitialized = true;
        }

        private static void loadMacAdamListfromDB()
        {
            try
            {
                string stmt = "select McAId, McAName, CCT, x0, y0, g11,g12, g22, CIE1964 from MacAdams where McAType ='ellipse' order by McAId";
                cmd.prep(stmt);
                cmd.Exec();

                listMacAdam.Clear();

                //-1 = keine Vorgabe
                MacAdams m = new MacAdams();
                m.McAId = -1;
                m.MCAName = "(no value)";
                m.CCT0 = 0;
                m.x0 = 0;
                m.y0 = 0;
                m.observer = Observer.Observer2deg;
                listMacAdam.Add(m);

                while (cmd.dr.Read())
                {
                    m = new MacAdams();
                    m.McAId = cmd.dr.GetInt32(0);
                    m.MCAName = cmd.dr.GetString(1);
                    m.CCT0 = cmd.dr.GetInt32(2);
                    m.x0 = cmd.dr.GetDouble(3);
                    m.y0 = cmd.dr.GetDouble(4);
                    m.g11 = cmd.dr.GetInt32(5);
                    m.g12 = cmd.dr.GetInt32(6);
                    m.g22 = cmd.dr.GetInt32(7);
                    m.observer = (Observer)cmd.dr.GetInt32(8);
                    listMacAdam.Add(m);
                }
            }
            finally
            {
                if (!cmd.dr.IsClosed)
                    cmd.dr.Close();
            }
        }

        private static void loadBinListfromDB()
        {
            //TBD
            try
            {
                //string stmt = "select McAId, McAName, CCT, x0, y0, g11,g12, g22, CIE1964 from MacAdams where McAType ='polygon' order by McAId";
                   string stmt = "select McAId, McAName, CCT, x0, y0, g11,g12, g22, CIE1964 from MacAdams where McAType ='ellipse' order by McAId";
                cmd.prep(stmt);
                cmd.Exec();

                listMacAdam.Clear();

                //-1 = keine Vorgabe
                MacAdams m = new MacAdams();
                m.McAId = -1;
                m.MCAName = "(no value)";
                m.CCT0 = 0;
                m.x0 = 0;
                m.y0 = 0;
                m.observer = Observer.Observer2deg;
                listMacAdam.Add(m);

                while (cmd.dr.Read())
                {
                    m = new MacAdams();
                    m.McAId = cmd.dr.GetInt32(0);
                    m.MCAName = cmd.dr.GetString(1);
                    m.CCT0 = cmd.dr.GetInt32(2);
                    m.x0 = cmd.dr.GetDouble(3);
                    m.y0 = cmd.dr.GetDouble(4);
                    m.g11 = cmd.dr.GetInt32(5);
                    m.g12 = cmd.dr.GetInt32(6);
                    m.g22 = cmd.dr.GetInt32(7);
                    m.observer = (Observer)cmd.dr.GetInt32(8);
                    listMacAdam.Add(m);
                }
            }
            finally
            {
                if (!cmd.dr.IsClosed)
                    cmd.dr.Close();
            }
        }

        private static void loadMacAdamListfromSource()
        {
            listMacAdam.Add(new MacAdams(10, "2000_CIE1931", 0.00,0.00, 2000, 440000, -186000, 270000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(20, "2500_CIE1931", 0.4770, 0.4137, 2500, 440000, -186000, 270000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(30, "2700_CIE1931", 0.4599, 0.4106, 2700, 440000, -186000, 270000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(40, "3000_CIE1931", 0.4369, 0.4041, 3000, 390000, -195000, 275000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(50, "3500_CIE1931", 0.4053, 0.3907, 3500, 380000, -200000, 250000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(60, "4000_CIE1931", 0.3804, 0.3767, 4000, 395000, -215000, 260000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(70, "4200_CIE1931", 0.3720, 0.3713, 4200, 395000, -215000, 260000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(80, "4500_CIE1931", 0.3608, 0.3635, 4500, 395000, -215000, 260000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(90, "5000_CIE1931", 0.3451, 0.3516, 5000, 560000, -250000, 280000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(100, "5500_CIE1931", 0.3324, 0.3410, 5500, 560000, -250000, 280000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(110, "5700_CIE1931", 0.3280, 0.3372, 5700, 560000, -250000, 280000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(120, "6000_CIE1931", 0.3221, 0.3318, 6000, 860000, -400000, 450000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(130, "6500_CIE1931", 0.3135, 0.3236, 6500, 860000, -400000, 450000, Observer.Observer2deg));
            listMacAdam.Add(new MacAdams(140, "7000_CIE1931", 0.3064, 0.3165, 7000, 860000, -400000, 450000, Observer.Observer2deg));
        }

        private static void loadBinListfromSource()
        {
            //TBD
        }

       private static double Planck(double lambda, double T)
       {
	        double c1, c2, ret;
	        c1 = 2*pi*planck_h* Math.Pow(licht_c,2);
	        c2 = planck_h*licht_c/(boltzmann_k);
            ret = c1 / Math.Pow(lambda, 5) * Math.Pow(Math.Exp(c2 / (lambda * T)) - 1, -1);
	        ret=ret*1e-9;

	        return ret;
        }

       public static CIECoords CCT2xy(double CCT, Observer obs = Observer.Observer2deg)
       {
           Dictionary<int, Tristimulus> cmf;
           Tristimulus T = new Tristimulus(0,0,0);
           double lambda, tmp, tmp2=0;
           CIECoords ret;

           if (!isInitialized) throw new ArgumentNullException("Colour Matching Functions not initialized!");

           if (obs == Observer.Observer10deg) cmf = CIEColorMatchingFunctions10;
           else cmf = CIEColorMatchingFunctions2;

           foreach (KeyValuePair<int, Tristimulus> pair in cmf)
           {
               lambda = pair.Key * 1e-9;
               tmp= Planck(lambda, CCT);

               tmp2 = tmp2 + tmp;
               T.X += tmp * pair.Value.X;
               T.Y += tmp * pair.Value.Y;
               T.Z += tmp * pair.Value.Z;
           }

           double sum = (T.X + T.Y + T.Z);
           ret.x = T.X / sum; ret.y = T.Y / sum; ret.z = T.Z / sum;

           return ret;
       }

       public static double xy2CCT(CIECoords cxy, int algorithm)
       {
	        //int ret=0;
	        double n=0;
            double CCT=-1;

	        //Nach Book Colorimetry von Robertson
	        // Zuerst mal berechnen
	        n=(cxy.x-0.332)/(cxy.y-0.1858);
	        CCT= (-437*n*n*n +3601* n*n-6861*n+5524.31);

	        if (algorithm==0) 
	        {
		        //Best Fit
		        //
		        if (CCT <= 1980) algorithm=2;
		        if ((CCT > 1980) && (CCT<=8400)) algorithm=1;
		        if (CCT > 8400) algorithm=3;
	        }


	        if (algorithm==1)
	        {
		        //Nach Book Colorimetry von Robertson

		        //Robertson lassen
	        }
	        else if (algorithm==2)
	        {
		        // Nach  Wikipadia:http://en.wikipedia.org/wiki/Color_temperature
		        n=(cxy.x-0.332)/(cxy.y-0.1858);
		        CCT= (-449* n*n*n+3525* n*n-6823.3*n+5520.33);
		        //ret=1;
	        }
	        else if (algorithm==3)
	        {
		        // Nach  Wikipadia:http://en.wikipedia.org/wiki/Color_temperature für CCT= 3-50k K
		        n=(cxy.x-0.3366) / (cxy.y-0.1735);
		        CCT=(-949.86315 + 6253.803 *Math.Exp(-n/0.92159) +  28.7060 * Math.Exp(-n / 0.20039) + 0.00004 * Math.Exp(-n / 0.07125));
		        //ret=1;
	        }
	        else
	        {

		        CCT=-1;
		        //ret=1;
	        }
	
	        return CCT;
       }

       public static double getMacAdamRadius(string binname, CIECoords cxy)
       {
           double ret=-1;
           var mc = listMacAdam.Find(item => item.MCAName == binname);

           if (!mc.Equals(null))
            ret = Math.Sqrt(mc.g11 * (cxy.x - mc.x0) * (cxy.x - mc.x0) + 2 * mc.g12 * (cxy.x - mc.x0) * (cxy.y - mc.y0) + mc.g22 * (cxy.y - mc.y0) * (cxy.y - mc.y0));

           return ret;
       }

       public static double getMacAdamRadius(MacAdams mca, CIECoords cxy)
       {
	        double   ret= Math.Sqrt(mca.g11*(cxy.x-mca.x0)*(cxy.x-mca.x0)+2*mca.g12*(cxy.x-mca.x0)*(cxy.y-mca.y0)+mca.g22*(cxy.y-mca.y0)*(cxy.y-mca.y0));
	        return ret;
       }

        public static CIECoords LTCCT2xy(double CCT)
        {
	        double dCCT=CCT/10000;
            CIECoords ccoord;
	        ccoord.x=-1;
	        ccoord.y=-1;
            ccoord.z=-1;


	        if ((CCT > 3000) && (CCT < 7000))
	        {
		        ccoord.x = 1.3688 * Math.Pow(dCCT,4) - 3.9201 * Math.Pow(dCCT,3) + 4.4378 * Math.Pow(dCCT,2) - 2.4615 * dCCT + 0.8708;
		        ccoord.y = -0.8846 * Math.Pow(dCCT,4) + 1.9613  * Math.Pow(dCCT,3) - 1.3591 * Math.Pow(dCCT,2) + 0.1029 * dCCT + 0.4501;
	        }
	        else if (CCT <=3000)
	        {
		        ccoord.x = 1.3688 * Math.Pow(dCCT,4) - 3.9201 * Math.Pow(dCCT,3) + 4.4378 * Math.Pow(dCCT,2) - 2.4615 * dCCT + 0.8708;
		        ccoord.y = -27.505 * Math.Pow(dCCT,4)  + 38.378 * Math.Pow(dCCT,3) - 20.397 * Math.Pow(dCCT,2) + 4.602 * dCCT + 0.0457;
	        }
	        else if (CCT>=7000)
	        {
		        ccoord.x = -0.1596 * Math.Pow(dCCT,3) + 0.5286 * Math.Pow(dCCT,2) - 0.6347 * dCCT + 0.5464;
		        ccoord.y = -0.1133 * Math.Pow(dCCT,3) + 0.3998 * Math.Pow(dCCT,2) - 0.5256 * dCCT + 0.5274;
	        }

            ccoord.z = 1 - ccoord.x - ccoord.y;

	        return ccoord;
        }

        private static int Windungszahl(PolygonBin bin, double x, double y)
        {
	        double dx=0, dy=0, k, d, tmp1;
	        int wn = 0;

	        //Eckpunkte prüfen
	        // "0"ter Punkt ist Ziel, deswegen von 1 weg zählen
	        for (int i=1; i<bin.npoints; i++)
	        {
		        dx = Math.Abs(x-bin.Polygon[i].x);
		        dy = Math.Abs(y-bin.Polygon[i].y);
		
		        //** wn <>0 heisst drinnen
		        if ((dx < ACCURACY) && (dy <ACCURACY) ) 
		        {
			        wn = 99;
			        return wn;
		        }
	        }

	        // Jetzt eigentlicher Algorithmus
	        double x1 = bin.Polygon[bin.npoints-2].x;
	        double y1 = bin.Polygon[bin.npoints-2].y;
	        double x2 = bin.Polygon[1].x;
	        double y2 = bin.Polygon[1].y;
	
	        bool bstartUeber = (y1 >= y)? true : false;
	        bool bendUeber;

	        for (int i=2; i<bin.npoints; i++)
	        {		
		        bendUeber = (y2 >= y)? true : false;		

		        if ( (bstartUeber != bendUeber) )//&& ( abs(x2-x1) > 0 ) )
		        {
			        if ( ((y2-y) * (x2-x1)) <= ((y2-y1) * (x2-x)) )
			        {
				        if (bendUeber) wn++;
			        }
			        else
			        {
				        if (!bendUeber) wn--;
			        }
		        }
		        else
		        {
			        // Check Gerade zwischen den Punkten x1, x2
			        if (Math.Abs(x2-x1)>0)
			        {
				        k=(y2-y1)/(x2-x1);
				        d=y1-k*x1;
				        tmp1= k*x+d;

				        if ( (Math.Abs(y-tmp1) < ACCURACY) && ( ((x>=x1) && (x<=x2)) ||  ((x>=x2) && (x<=x1)) ) )
				        {
					        wn=99;
					        return wn;
				        };
			        }
			        else // k= unendlich
			        {
				        if ( ( ((y>=y1) && (y<=y2)) ||  ((y>=y2) && (y<=y1)) ) && ( ((x>=x1) && (x<=x2)) ||  ((x>=x2) && (x<=x1)) ) )
				        {
					        wn=99;
					        return wn;
				        };
			        }				
		        }

		        bstartUeber = bendUeber;
		        y1=y2;
		        x1=x2;
		        x2=bin.Polygon[i].x;
		        y2=bin.Polygon[i].y;
	        }

	        return wn;
        }

        public static MacAdams getMacAdamDefinition(string binname)
        {
            var mc = listMacAdam.Find(item => item.MCAName == binname);
            return mc;
        }

       private static void InitCMF()
       {
           int wl = 360;
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001299, 0.000003917, 0.0006061));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000145847, 0.000004393581, 0.0006808792));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001638021, 0.000004929604, 0.0007651456));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001840037, 0.000005532136, 0.0008600124));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002066902, 0.000006208245, 0.0009665928));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002321, 0.000006965, 0.001086));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000260728, 0.000007813219, 0.001220586));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000293075, 0.000008767336, 0.001372729));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000329388, 0.000009839844, 0.001543579));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000369914, 0.00001104323, 0.001734286));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0004149, 0.00001239, 0.001946));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0004641587, 0.00001388641, 0.002177777));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000518986, 0.00001555728, 0.002435809));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000581854, 0.00001744296, 0.002731953));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0006552347, 0.00001958375, 0.003078064));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0007416, 0.00002202, 0.003486));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0008450296, 0.00002483965, 0.003975227));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0009645268, 0.00002804126, 0.00454088));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001094949, 0.00003153104, 0.00515832));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001231154, 0.00003521521, 0.005802907));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001368, 0.000039, 0.006450001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00150205, 0.0000428264, 0.007083216));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001642328, 0.0000469146, 0.007745488));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001802382, 0.0000515896, 0.008501152));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001995757, 0.0000571764, 0.009414544));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002236, 0.000064, 0.01054999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002535385, 0.00007234421, 0.0119658));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002892603, 0.00008221224, 0.01365587));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003300829, 0.00009350816, 0.01558805));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003753236, 0.0001061361, 0.01773015));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.004243, 0.00012, 0.02005001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.004762389, 0.000134984, 0.02251136));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.005330048, 0.000151492, 0.02520288));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.005978712, 0.000170208, 0.02827972));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.006741117, 0.000191816, 0.03189704));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00765, 0.000217, 0.03621));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.008751373, 0.0002469067, 0.04143771));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01002888, 0.00028124, 0.04750372));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0114217, 0.00031852, 0.05411988));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01286901, 0.0003572667, 0.06099803));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01431, 0.000396, 0.06785001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01570443, 0.0004337147, 0.07448632));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01714744, 0.000473024, 0.08136156));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01878122, 0.000517876, 0.08915364));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02074801, 0.0005722187, 0.09854048));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02319, 0.00064, 0.1102));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02620736, 0.00072456, 0.1246133));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02978248, 0.0008255, 0.1417017));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03388092, 0.00094116, 0.1613035));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03846824, 0.00106988, 0.1832568));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04351, 0.00121, 0.2074));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0489956, 0.001362091, 0.2336921));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0550226, 0.001530752, 0.2626114));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0617188, 0.001720368, 0.2947746));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.069212, 0.001935323, 0.3307985));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07763, 0.00218, 0.3713));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.08695811, 0.0024548, 0.4162091));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.09717672, 0.002764, 0.4654642));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1084063, 0.0031178, 0.5196948));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1207672, 0.0035264, 0.5795303));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.13438, 0.004, 0.6456));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1493582, 0.00454624, 0.7184838));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1653957, 0.00515932, 0.7967133));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1819831, 0.00582928, 0.8778459));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.198611, 0.00654616, 0.959439));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.21477, 0.0073, 1.0390501));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2301868, 0.008086507, 1.1153673));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2448797, 0.00890872, 1.1884971));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2587773, 0.00976768, 1.2581233));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2718079, 0.01066443, 1.3239296));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2839, 0.0116, 1.3856));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2949438, 0.01257317, 1.4426352));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3048965, 0.01358272, 1.4948035));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3137873, 0.01462968, 1.5421903));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3216454, 0.01571509, 1.5848807));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3285, 0.01684, 1.62296));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3343513, 0.01800736, 1.6564048));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3392101, 0.01921448, 1.6852959));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3431213, 0.02045392, 1.7098745));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3461296, 0.02171824, 1.7303821));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.34828, 0.023, 1.74706));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3495999, 0.02429461, 1.7600446));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3501474, 0.02561024, 1.7696233));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.350013, 0.02695857, 1.7762637));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.349287, 0.02835125, 1.7804334));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.34806, 0.0298, 1.7826));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3463733, 0.03131083, 1.7829682));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3442624, 0.03288368, 1.7816998));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3418088, 0.03452112, 1.7791982));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3390941, 0.03622571, 1.7758671));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3362, 0.038, 1.77211));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3331977, 0.03984667, 1.7682589));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3300411, 0.041768, 1.764039));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3266357, 0.043766, 1.7589438));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3228868, 0.04584267, 1.7524663));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3187, 0.048, 1.7441));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3140251, 0.05024368, 1.7335595));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.308884, 0.05257304, 1.7208581));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3032904, 0.05498056, 1.7059369));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2972579, 0.05745872, 1.6887372));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2908, 0.06, 1.6692));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2839701, 0.06260197, 1.6475287));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2767214, 0.06527752, 1.6234127));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2689178, 0.06804208, 1.5960223));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2604227, 0.07091109, 1.564528));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2511, 0.0739, 1.5281));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2408475, 0.077016, 1.4861114));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2298512, 0.0802664, 1.4395215));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2184072, 0.0836668, 1.3898799));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2068115, 0.0872328, 1.3387362));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.19536, 0.09098, 1.28764));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1842136, 0.09491755, 1.2374223));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1733273, 0.09904584, 1.1878243));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1626881, 0.1033674, 1.1387611));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1522833, 0.1078846, 1.090148));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1421, 0.1126, 1.0419));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1321786, 0.117532, 0.9941976));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1225696, 0.1226744, 0.9473473));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1132752, 0.1279928, 0.9014531));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1042979, 0.1334528, 0.8566193));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.09564, 0.13902, 0.8129501));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.08729955, 0.1446764, 0.7705173));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07930804, 0.1504693, 0.7294448));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07171776, 0.1564619, 0.6899136));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.06458099, 0.1627177, 0.6521049));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05795001, 0.1693, 0.6162));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05186211, 0.1762431, 0.5823286));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04628152, 0.1835581, 0.5504162));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04115088, 0.1912735, 0.5203376));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03641283, 0.199418, 0.4919673));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03201, 0.20802, 0.46518));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0279172, 0.2171199, 0.4399246));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0241444, 0.2267345, 0.4161836));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.020687, 0.2368571, 0.3938822));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0175404, 0.2474812, 0.3729459));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0147, 0.2586, 0.3533));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01216179, 0.2701849, 0.3348578));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00991996, 0.2822939, 0.3175521));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00796724, 0.2950505, 0.3013375));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.006296346, 0.308578, 0.2861686));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0049, 0.323, 0.272));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003777173, 0.3384021, 0.2588171));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00294532, 0.3546858, 0.2464838));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00242488, 0.3716986, 0.2347718));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002236293, 0.3892875, 0.2234533));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0024, 0.4073, 0.2123));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00292552, 0.4256299, 0.2011692));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00383656, 0.4443096, 0.1901196));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00517484, 0.4633944, 0.1792254));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00698208, 0.4829395, 0.1685608));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0093, 0.503, 0.1582));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01214949, 0.5235693, 0.1481383));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01553588, 0.544512, 0.1383758));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01947752, 0.56569, 0.1289942));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02399277, 0.5869653, 0.1200751));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0291, 0.6082, 0.1117));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03481485, 0.6293456, 0.1039048));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04112016, 0.6503068, 0.09666748));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04798504, 0.6708752, 0.08998272));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05537861, 0.6908424, 0.08384531));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.06327, 0.71, 0.07824999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07163501, 0.7281852, 0.07320899));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.08046224, 0.7454636, 0.06867816));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.08973996, 0.7619694, 0.06456784));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.09945645, 0.7778368, 0.06078835));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1096, 0.7932, 0.05725001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1201674, 0.8081104, 0.05390435));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1311145, 0.8224962, 0.05074664));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1423679, 0.8363068, 0.04775276));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1538542, 0.8494916, 0.04489859));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1655, 0.862, 0.04216));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1772571, 0.8738108, 0.03950728));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.18914, 0.8849624, 0.03693564));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2011694, 0.8954936, 0.03445836));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2133658, 0.9054432, 0.03208872));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2257499, 0.9148501, 0.02984));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2383209, 0.9237348, 0.02771181));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2510668, 0.9320924, 0.02569444));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2639922, 0.9399226, 0.02378716));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2771017, 0.9472252, 0.02198925));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2904, 0.954, 0.0203));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3038912, 0.9602561, 0.01871805));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3175726, 0.9660074, 0.01724036));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3314384, 0.9712606, 0.01586364));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3454828, 0.9760225, 0.01458461));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3597, 0.9803, 0.0134));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3740839, 0.9840924, 0.01230723));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3886396, 0.9874182, 0.01130188));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4033784, 0.9903128, 0.01037792));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4183115, 0.9928116, 0.009529306));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4334499, 0.9949501, 0.008749999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4487953, 0.9967108, 0.0080352));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.464336, 0.9980983, 0.0073816));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.480064, 0.999112, 0.0067854));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4959713, 0.9997482, 0.0062428));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5120501, 1, 0.005749999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5282959, 0.9998567, 0.0053036));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5446916, 0.9993046, 0.0048998));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5612094, 0.9983255, 0.0045342));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5778215, 0.9968987, 0.0042024));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5945, 0.995, 0.0039));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6112209, 0.9926005, 0.0036232));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6279758, 0.9897426, 0.0033706));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6447602, 0.9864444, 0.0031414));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6615697, 0.9827241, 0.0029348));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6784, 0.9786, 0.002749999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6952392, 0.9740837, 0.0025852));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7120586, 0.9691712, 0.0024386));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7288284, 0.9638568, 0.0023094));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7455188, 0.9581349, 0.0021968));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7621, 0.952, 0.0021));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7785432, 0.9454504, 0.002017733));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7948256, 0.9384992, 0.0019482));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8109264, 0.9311628, 0.0018898));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8268248, 0.9234576, 0.001840933));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8425, 0.9154, 0.0018));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8579325, 0.9070064, 0.001766267));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8730816, 0.8982772, 0.0017378));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8878944, 0.8892048, 0.0017112));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9023181, 0.8797816, 0.001683067));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9163, 0.87, 0.001650001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9297995, 0.8598613, 0.001610133));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9427984, 0.849392, 0.0015644));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9552776, 0.838622, 0.0015136));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9672179, 0.8275813, 0.001458533));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9786, 0.8163, 0.0014));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9893856, 0.8047947, 0.001336667));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9995488, 0.793082, 0.00127));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0090892, 0.781192, 0.001205));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0180064, 0.7691547, 0.001146667));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0263, 0.757, 0.0011));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0339827, 0.7447541, 0.0010688));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.040986, 0.7324224, 0.0010494));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.047188, 0.7200036, 0.0010356));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0524667, 0.7074965, 0.0010212));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0567, 0.6949, 0.001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0597944, 0.6822192, 0.00096864));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0617992, 0.6694716, 0.00092992));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0628068, 0.6566744, 0.00088688));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0629096, 0.6438448, 0.00084256));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0622, 0.631, 0.0008));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0607352, 0.6181555, 0.00076096));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0584436, 0.6053144, 0.00072368));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0552244, 0.5924756, 0.00068592));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0509768, 0.5796379, 0.00064544));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0456, 0.5668, 0.0006));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0390369, 0.5539611, 0.0005478667));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0313608, 0.5411372, 0.0004916));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0226662, 0.5283528, 0.0004354));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0130477, 0.5156323, 0.0003834667));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(1.0026, 0.503, 0.00034));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9913675, 0.4904688, 0.0003072533));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9793314, 0.4780304, 0.00028316));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9664916, 0.4656776, 0.00026544));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9528479, 0.4534032, 0.0002518133));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.9384, 0.4412, 0.00024));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.923194, 0.42908, 0.0002295467));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.907244, 0.417036, 0.00022064));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.890502, 0.405032, 0.00021196));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.87292, 0.393032, 0.0002021867));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.8544499, 0.381, 0.00019));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.835084, 0.3689184, 0.0001742133));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.814946, 0.3568272, 0.00015564));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.794186, 0.3447768, 0.00013596));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.772954, 0.3328176, 0.0001168533));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7514, 0.321, 0.0001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7295836, 0.3093381, 0.00008613333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.7075888, 0.2978504, 0.0000746));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6856022, 0.2865936, 0.000065));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6638104, 0.2756245, 0.00005693333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6424, 0.265, 0.00004999999));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6215149, 0.2547632, 0.00004416));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.6011138, 0.2448896, 0.00003948));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5811052, 0.2353344, 0.00003572));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5613977, 0.2260528, 0.00003264));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5419, 0.217, 0.00003));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5225995, 0.2081616, 0.00002765333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.5035464, 0.1995488, 0.00002556));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4847436, 0.1911552, 0.00002364));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4661939, 0.1829744, 0.00002181333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4479, 0.175, 0.00002));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.4298613, 0.1672235, 0.00001813333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.412098, 0.1596464, 0.0000162));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.394644, 0.1522776, 0.0000142));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3775333, 0.1451259, 0.00001213333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3608, 0.1382, 0.00001));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3444563, 0.1315003, 0.000007733333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3285168, 0.1250248, 0.0000054));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.3130192, 0.1187792, 0.0000032));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2980011, 0.1127691, 0.000001333333));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2835, 0.107, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2695448, 0.1014762, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2561184, 0.09618864, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2431896, 0.09112296, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2307272, 0.08626485, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2187, 0.0816, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.2070971, 0.07712064, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1959232, 0.07282552, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1851708, 0.06871008, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1748323, 0.06476976, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1649, 0.061, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1553667, 0.05739621, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.14623, 0.05395504, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.13749, 0.05067376, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1291467, 0.04754965, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1212, 0.04458, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.1136397, 0.04175872, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.106465, 0.03908496, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.09969044, 0.03656384, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.09333061, 0.03420048, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0874, 0.032, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.08190096, 0.02996261, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07680428, 0.02807664, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.07207712, 0.02632936, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.06768664, 0.02470805, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0636, 0.0232, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05980685, 0.02180077, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05628216, 0.02050112, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.05297104, 0.01928108, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04981861, 0.01812069, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04677, 0.017, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04378405, 0.01590379, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.04087536, 0.01483718, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03807264, 0.01381068, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03540461, 0.01283478, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0329, 0.01192, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.03056419, 0.01106831, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02838056, 0.01027339, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02634484, 0.009533311, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02445275, 0.008846157, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0227, 0.00821, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.02108429, 0.007623781, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01959988, 0.007085424, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01823732, 0.006591476, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01698717, 0.006138485, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01584, 0.005723, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01479064, 0.005343059, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01383132, 0.004995796, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01294868, 0.004676404, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0121292, 0.004380075, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01135916, 0.004102, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.01062935, 0.003838453, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.009938846, 0.003589099, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.009288422, 0.003354219, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.008678854, 0.003134093, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.008110916, 0.002929, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.007582388, 0.002738139, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.007088746, 0.002559876, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.006627313, 0.002393244, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.006195408, 0.002237275, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.005790346, 0.002091, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.005409826, 0.001953587, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.005052583, 0.00182458, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.004717512, 0.00170358, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.004403507, 0.001590187, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.004109457, 0.001484, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003833913, 0.001384496, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003575748, 0.001291268, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003334342, 0.001204092, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.003109075, 0.001122744, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002899327, 0.001047, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002704348, 0.0009765896, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00252302, 0.0009111088, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002354168, 0.0008501332, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.002196616, 0.0007932384, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00204919, 0.00074, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00191096, 0.0006900827, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001781438, 0.00064331, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00166011, 0.000599496, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001546459, 0.0005584547, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001439971, 0.00052, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001340042, 0.0004839136, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001246275, 0.0004500528, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.001158471, 0.0004183452, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00107643, 0.0003887184, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0009999493, 0.0003611, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0009287358, 0.0003353835, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0008624332, 0.0003114404, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0008007503, 0.0002891656, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000743396, 0.0002684539, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0006900786, 0.0002492, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0006405156, 0.0002313019, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0005945021, 0.0002146856, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0005518646, 0.0001992884, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000512429, 0.0001850475, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0004760213, 0.0001719, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0004424536, 0.0001597781, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0004115117, 0.0001486044, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0003829814, 0.0001383016, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0003566491, 0.0001287925, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0003323011, 0.00012, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0003097586, 0.0001118595, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002888871, 0.0001043224, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002695394, 0.0000973356, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002515682, 0.00009084587, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002348261, 0.0000848, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000219171, 0.00007914667, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0002045258, 0.000073858, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001908405, 0.000068916, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001780654, 0.00006430267, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001661505, 0.00006, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001550236, 0.00005598187, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001446219, 0.0000522256, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001349098, 0.0000487184, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000125852, 0.00004544747, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000117413, 0.0000424, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001095515, 0.00003956104, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0001022245, 0.00003691512, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00009539445, 0.00003444868, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0000890239, 0.00003214816, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00008307527, 0.00003, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00007751269, 0.00002799125, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00007231304, 0.00002611356, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00006745778, 0.00002436024, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00006292844, 0.00002272461, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00005870652, 0.0000212, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00005477028, 0.00001977855, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00005109918, 0.00001845285, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00004767654, 0.00001721687, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00004448567, 0.00001606459, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00004150994, 0.00001499, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00003873324, 0.00001398728, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00003614203, 0.00001305155, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00003372352, 0.00001217818, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00003146487, 0.00001136254, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00002935326, 0.0000106, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00002737573, 0.000009885877, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00002552433, 0.000009217304, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00002379376, 0.000008592362, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0000221787, 0.000008009133, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00002067383, 0.0000074657, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001927226, 0.000006959567, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.0000179664, 0.000006487995, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001674991, 0.000006048699, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001561648, 0.000005639396, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001455977, 0.0000052578, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001357387, 0.000004901771, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001265436, 0.00000456972, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001179723, 0.000004260194, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001099844, 0.000003971739, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00001025398, 0.0000037029, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000009559646, 0.000003452163, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000008912044, 0.000003218302, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000008308358, 0.0000030003, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000007745769, 0.000002797139, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000007221456, 0.0000026078, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000006732475, 0.00000243122, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000006276423, 0.000002266531, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000005851304, 0.000002113013, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000005455118, 0.000001969943, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000005085868, 0.0000018366, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000004741466, 0.00000171223, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000004420236, 0.000001596228, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000004120783, 0.00000148809, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000003841716, 0.000001387314, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000003581652, 0.0000012934, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000003339127, 0.00000120582, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000003112949, 0.000001124143, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002902121, 0.000001048009, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002705645, 0.000000977058, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002522525, 0.00000091093, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002351726, 0.000000849251, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002192415, 0.000000791721, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000002043902, 0.00000073809, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001905497, 0.00000068811, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001776509, 0.00000064153, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001656215, 0.00000059809, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001544022, 0.000000557575, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.00000143944, 0.000000519808, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001341977, 0.000000484612, 0));
           CIEColorMatchingFunctions2.Add(wl++, new Tristimulus(0.000001251141, 0.00000045181, 0));

           wl = 360;
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000001222, 0.000000013398, 0.000000535027));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000000185138, 0.000000020294, 0.00000081072));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000027883, 0.00000003056, 0.0000012212));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000041747, 0.00000004574, 0.0000018287));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000062133, 0.00000006805, 0.0000027222));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000091927, 0.00000010065, 0.0000040283));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000135198, 0.00000014798, 0.0000059257));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000197654, 0.00000021627, 0.0000086651));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000028725, 0.0000003142, 0.000012596));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000041495, 0.0000004537, 0.000018201));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000059586, 0.0000006511, 0.0000261437));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000085056, 0.0000009288, 0.00003733));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000120686, 0.0000013175, 0.000052987));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000170226, 0.0000018572, 0.000074764));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000023868, 0.000002602, 0.00010487));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000033266, 0.000003625, 0.00014622));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000046087, 0.000005019, 0.00020266));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000063472, 0.000006907, 0.00027923));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000086892, 0.000009449, 0.00038245));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000118246, 0.000012848, 0.00052072));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000159952, 0.000017364, 0.000704776));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00021508, 0.000023327, 0.00094823));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00028749, 0.00003115, 0.0012682));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00038199, 0.00004135, 0.0016861));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00050455, 0.00005456, 0.0022285));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00066244, 0.00007156, 0.0029278));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0008645, 0.0000933, 0.0038237));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0011215, 0.00012087, 0.0049642));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00144616, 0.00015564, 0.0064067));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00185359, 0.0001992, 0.0082193));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0023616, 0.0002534, 0.0104822));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0029906, 0.0003202, 0.013289));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0037645, 0.0004024, 0.016747));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0047102, 0.0005023, 0.02098));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0058581, 0.0006232, 0.026127));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0072423, 0.0007685, 0.032344));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0088996, 0.0009417, 0.039802));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0108709, 0.0011478, 0.048691));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0131989, 0.0013903, 0.05921));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0159292, 0.001674, 0.071576));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0191097, 0.0020044, 0.0860109));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.022788, 0.002386, 0.10274));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.027011, 0.002822, 0.122));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.031829, 0.003319, 0.14402));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.037278, 0.00388, 0.16899));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0434, 0.004509, 0.19712));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.050223, 0.005209, 0.22857));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.057764, 0.005985, 0.26347));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.066038, 0.006833, 0.3019));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.075033, 0.007757, 0.34387));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.084736, 0.008756, 0.389366));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.095041, 0.009816, 0.43797));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.105836, 0.010918, 0.48922));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.117066, 0.012058, 0.5429));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.128682, 0.013237, 0.59881));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.140638, 0.014456, 0.65676));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.152893, 0.015717, 0.71658));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.165416, 0.017025, 0.77812));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.178191, 0.018399, 0.84131));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.191214, 0.019848, 0.90611));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.204492, 0.021391, 0.972542));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.21765, 0.022992, 1.0389));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.230267, 0.024598, 1.1031));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.242311, 0.026213, 1.1651));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.253793, 0.027841, 1.2249));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.264737, 0.029497, 1.2825));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.275195, 0.031195, 1.3382));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.285301, 0.032927, 1.3926));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.295143, 0.034738, 1.4461));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.304869, 0.036654, 1.4994));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.314679, 0.038676, 1.55348));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.324355, 0.040792, 1.6072));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.33357, 0.042946, 1.6589));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.342243, 0.045114, 1.7082));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.350312, 0.047333, 1.7548));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.357719, 0.049602, 1.7985));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.364482, 0.051934, 1.8392));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.370493, 0.054337, 1.8766));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.375727, 0.056822, 1.9105));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.380158, 0.059399, 1.9408));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.383734, 0.062077, 1.96728));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.386327, 0.064737, 1.9891));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.387858, 0.067285, 2.0057));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.388396, 0.069764, 2.0174));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.387978, 0.072218, 2.0244));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.386726, 0.074704, 2.0273));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.384696, 0.077272, 2.0264));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.382006, 0.079979, 2.0223));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.378709, 0.082874, 2.0153));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.374915, 0.086, 2.006));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.370702, 0.089456, 1.9948));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.366089, 0.092947, 1.9814));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.361045, 0.096275, 1.9653));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.355518, 0.099535, 1.9464));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.349486, 0.102829, 1.9248));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.342957, 0.106256, 1.9007));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.335893, 0.109901, 1.8741));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.328284, 0.113835, 1.8451));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.32015, 0.118167, 1.8139));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.311475, 0.122932, 1.7806));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.302273, 0.128201, 1.74537));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.292858, 0.133457, 1.7091));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.283502, 0.138323, 1.6723));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.274044, 0.143042, 1.6347));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.264263, 0.147787, 1.5956));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.254085, 0.152761, 1.5549));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.243392, 0.158102, 1.5122));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.232187, 0.163941, 1.4673));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.220488, 0.170362, 1.4199));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.208198, 0.177425, 1.37));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.195618, 0.18519, 1.31756));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.183034, 0.193025, 1.2624));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.170222, 0.200313, 1.205));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.157348, 0.207156, 1.1466));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.14465, 0.213644, 1.088));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.132349, 0.21994, 1.0302));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.120584, 0.22617, 0.97383));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.109456, 0.232467, 0.91943));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.099042, 0.239025, 0.86746));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.089388, 0.245997, 0.81828));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.080507, 0.253589, 0.772125));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.072034, 0.261876, 0.72829));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.06371, 0.270643, 0.68604));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.055694, 0.279645, 0.64553));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.048117, 0.288694, 0.60685));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.041072, 0.297665, 0.57006));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.034642, 0.306469, 0.53522));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.028896, 0.315035, 0.50234));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.023876, 0.323335, 0.4714));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.019628, 0.331366, 0.44239));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.016172, 0.339133, 0.415254));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0133, 0.34786, 0.390024));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.010759, 0.358326, 0.366399));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.008542, 0.370001, 0.344015));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.006661, 0.382464, 0.322689));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.005132, 0.395379, 0.302356));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.003982, 0.408482, 0.283036));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.003239, 0.421588, 0.264816));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.002934, 0.434619, 0.247848));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.003114, 0.447601, 0.232318));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.003816, 0.460777, 0.218502));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.005095, 0.47434, 0.205851));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.006936, 0.4882, 0.193596));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.009299, 0.50234, 0.181736));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.012147, 0.51674, 0.170281));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.015444, 0.53136, 0.159249));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.019156, 0.54619, 0.148673));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.02325, 0.56118, 0.138609));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.02769, 0.57629, 0.129096));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.032444, 0.5915, 0.120215));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.037465, 0.606741, 0.112044));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.042956, 0.62215, 0.10471));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.049114, 0.63783, 0.098196));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.05592, 0.65371, 0.092361));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.063349, 0.66968, 0.087088));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.071358, 0.68566, 0.082248));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.079901, 0.70155, 0.077744));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.088909, 0.71723, 0.073456));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.098293, 0.73257, 0.069268));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.107949, 0.74746, 0.06506));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.117749, 0.761757, 0.060709));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.127839, 0.77534, 0.056457));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.13845, 0.78822, 0.052609));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.149516, 0.80046, 0.049122));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.161041, 0.81214, 0.045954));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.172953, 0.82333, 0.04305));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.185209, 0.83412, 0.040368));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.197755, 0.8446, 0.037839));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.210538, 0.85487, 0.035384));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.22346, 0.86504, 0.032949));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.236491, 0.875211, 0.030451));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.249633, 0.88537, 0.028029));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.262972, 0.89537, 0.025862));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.276515, 0.90515, 0.02392));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.290269, 0.91465, 0.022174));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.304213, 0.92381, 0.020584));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.318361, 0.93255, 0.019127));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.332705, 0.94081, 0.01774));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.347232, 0.94852, 0.016403));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.361926, 0.9556, 0.015064));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.376772, 0.961988, 0.013676));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.391683, 0.96754, 0.012308));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.406594, 0.97223, 0.011056));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.421539, 0.97617, 0.009915));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.436517, 0.97946, 0.008872));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.451584, 0.9822, 0.007918));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.466782, 0.98452, 0.00703));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.482147, 0.98652, 0.006223));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.497738, 0.98832, 0.005453));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.513606, 0.99002, 0.004714));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.529826, 0.991761, 0.003988));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.54644, 0.99353, 0.003289));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.563426, 0.99523, 0.002646));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.580726, 0.99677, 0.002063));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.59829, 0.99809, 0.001533));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.616053, 0.99911, 0.001091));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.633948, 0.99977, 0.000711));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.651901, 1, 0.000407));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.669824, 0.99971, 0.000184));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.687632, 0.99885, 0.000047));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.705224, 0.99734, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.722773, 0.99526, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.740483, 0.99274, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.758273, 0.98975, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.776083, 0.9863, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.793832, 0.98238, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.811436, 0.97798, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.828822, 0.97311, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.845879, 0.96774, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.862525, 0.96189, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.878655, 0.955552, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.894208, 0.948601, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.909206, 0.940981, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.923672, 0.932798, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.937638, 0.924158, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.951162, 0.915175, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.964283, 0.905954, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.977068, 0.896608, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.98959, 0.887249, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.00191, 0.877986, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.01416, 0.868934, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0265, 0.860164, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0388, 0.851519, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.051, 0.842963, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0629, 0.834393, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0743, 0.825623, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0852, 0.816764, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0952, 0.807544, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1042, 0.797947, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.112, 0.787893, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.11852, 0.777405, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1238, 0.76649, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.128, 0.755309, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1311, 0.743845, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1332, 0.73219, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1343, 0.720353, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1343, 0.708281, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1333, 0.696055, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1312, 0.683621, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1281, 0.671048, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.12399, 0.658341, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1189, 0.645545, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1129, 0.632718, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.1059, 0.619815, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.098, 0.606887, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0891, 0.593878, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0792, 0.580781, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0684, 0.567653, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0567, 0.55449, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.044, 0.541228, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.03048, 0.527963, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.016, 0.514634, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(1.0008, 0.501363, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.98479, 0.488124, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.96808, 0.474935, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.95074, 0.461834, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.9328, 0.448823, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.91434, 0.435917, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.89539, 0.423153, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.87603, 0.410526, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.856297, 0.398057, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.83635, 0.385835, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.81629, 0.373951, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.79605, 0.362311, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.77561, 0.350863, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.75493, 0.339554, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.73399, 0.328309, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.71278, 0.317118, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.69129, 0.305936, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.66952, 0.294737, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.647467, 0.283493, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.62511, 0.272222, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.60252, 0.26099, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.57989, 0.249877, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.55737, 0.238946, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.53511, 0.228254, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.51324, 0.217853, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.49186, 0.20778, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.47108, 0.198072, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.45096, 0.188748, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.431567, 0.179828, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.41287, 0.171285, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.39475, 0.163059, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.37721, 0.155151, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.36019, 0.147535, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.34369, 0.140211, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.32769, 0.13317, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.31217, 0.1264, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.29711, 0.119892, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.2825, 0.11364, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.268329, 0.107633, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.25459, 0.10187, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.2413, 0.096347, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.22848, 0.091063, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.21614, 0.08601, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.2043, 0.081187, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.19295, 0.076583, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.18211, 0.072198, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.17177, 0.068024, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.16192, 0.064052, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.152568, 0.060281, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.14367, 0.056697, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.1352, 0.053292, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.12713, 0.050059, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.11948, 0.046998, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.11221, 0.044096, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.10531, 0.041345, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.098786, 0.0387507, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.09261, 0.0362978, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.086773, 0.0339832, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0812606, 0.0318004, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.076048, 0.0297395, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.071114, 0.0277918, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.066454, 0.0259551, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.062062, 0.0242263, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.05793, 0.0226017, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.05405, 0.0210779, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.050412, 0.0196505, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.047006, 0.0183153, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.043823, 0.0170686, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0408508, 0.0159051, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.038072, 0.0148183, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.035468, 0.0138008, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.033031, 0.0128495, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.030753, 0.0119607, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.028623, 0.0111303, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.026635, 0.0103555, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.024781, 0.0096332, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.023052, 0.0089599, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.021441, 0.0083324, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0199413, 0.0077488, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.018544, 0.0072046, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.017241, 0.0066975, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.016027, 0.0062251, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.014896, 0.005785, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.013842, 0.0053751, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.012862, 0.0049941, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.011949, 0.0046392, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0111, 0.0043093, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.010311, 0.0040028, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00957688, 0.00371774, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.008894, 0.00345262, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0082581, 0.00320583, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0076664, 0.00297623, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0071163, 0.00276281, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0066052, 0.00256456, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0061306, 0.00238048, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0056903, 0.00220971, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0052819, 0.00205132, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0049033, 0.00190449, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00455263, 0.00176847, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0042275, 0.00164236, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0039258, 0.00152535, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0036457, 0.00141672, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0033859, 0.00131595, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0031447, 0.00122239, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0029208, 0.00113555, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.002713, 0.00105494, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0025202, 0.00098014, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0023411, 0.00091066, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00217496, 0.00084619, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0020206, 0.00078629, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0018773, 0.00073068, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0017441, 0.00067899, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0016205, 0.00063101, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0015057, 0.00058644, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0013992, 0.00054511, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0013004, 0.00050672, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0012087, 0.00047111, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0011236, 0.00043805, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00104476, 0.00040741, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00097156, 0.000378962, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0009036, 0.000352543, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00084048, 0.000328001, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00078187, 0.000305208, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00072745, 0.000284041, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0006769, 0.000264375, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00062996, 0.000246109, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00058637, 0.000229143, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00054587, 0.000213376, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000508258, 0.00019873, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0004733, 0.000185115, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0004408, 0.000172454, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00041058, 0.000160678, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00038249, 0.00014973, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00035638, 0.00013955, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00033211, 0.000130086, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00030955, 0.00012129, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00028858, 0.000113106, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00026909, 0.000105501, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000250969, 0.000098428, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00023413, 0.000091853, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00021847, 0.000085738, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00020391, 0.000080048, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00019035, 0.000074751, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00017773, 0.000069819, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00016597, 0.000065222, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00015502, 0.000060939, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0001448, 0.000056942, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00013528, 0.000053217, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00012639, 0.000049737, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0001181, 0.000046491, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00011037, 0.000043464, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00010315, 0.000040635, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000096427, 0.000038, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000090151, 0.0000355405, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000084294, 0.0000332448, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00007883, 0.0000311006, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000073729, 0.000029099, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000068969, 0.0000272307, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000645258, 0.000025486, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000060376, 0.0000238561, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000565, 0.0000223332, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00005288, 0.0000209104, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000049498, 0.0000195808, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000046339, 0.0000183384, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000043389, 0.0000171777, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000040634, 0.0000160934, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00003806, 0.00001508, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000035657, 0.0000141336, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000334117, 0.000013249, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000031315, 0.0000124226, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000029355, 0.0000116499, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000027524, 0.0000109277, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000025811, 0.0000102519, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000024209, 0.0000096196, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000022711, 0.0000090281, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000021308, 0.000008474, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000019994, 0.0000079548, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000018764, 0.0000074686, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000176115, 0.0000070128, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000016532, 0.0000065858, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000015521, 0.0000061857, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000014574, 0.0000058107, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000013686, 0.000005459, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000012855, 0.0000051298, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000012075, 0.0000048206, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000011345, 0.0000045312, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000010659, 0.0000042591, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000010017, 0.0000040042, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000941363, 0.00000376473, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000088479, 0.00000353995, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000083171, 0.00000332914, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000007819, 0.00000313115, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000073516, 0.00000294529, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000006913, 0.00000277081, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000065015, 0.00000260705, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000061153, 0.00000245329, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000057529, 0.00000230894, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000054127, 0.00000217338, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000509347, 0.00000204613, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000047938, 0.00000192662, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000045125, 0.0000018144, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000042483, 0.00000170895, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000040002, 0.00000160988, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000037671, 0.00000151677, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000003548, 0.00000142921, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000033421, 0.00000134686, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000031485, 0.00000126945, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000029665, 0.00000119662, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000279531, 0.00000112809, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000026345, 0.00000106368, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000024834, 0.00000100313, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000023414, 0.00000094622, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000022078, 0.00000089263, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.000002082, 0.00000084216, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000019636, 0.00000079464, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000018519, 0.00000074978, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000017465, 0.00000070744, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.0000016471, 0.00000066748, 0));
           CIEColorMatchingFunctions10.Add(wl++, new Tristimulus(0.00000155314, 0.0000006297, 0));
       }

    }
}
