using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
//using System.Windows.Input;

//TO-DO: 1. Multi branches on each side
//       2. Fix face wall view

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        Pen blackPen = new Pen(Color.Black, 1);
        Pen bluePen = new Pen(Color.Blue, 3);
        Pen grayPen = new Pen(Color.Gray, 3);

        List<wall> walls = new List<wall>();

        private const int map_x = 10;
        private const int map_y = 10;

        int[,] map = new int[map_x, map_y]{
                                  {1,1,1,0,0,0,1,1,1,0},
                                  {1,0,1,1,1,1,1,0,1,0},
                                  {1,0,0,0,0,0,0,1,1,0},
                                  {1,1,1,1,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,1,0},
                                  {0,0,0,0,1,0,0,0,1,0},
                                  {0,0,0,0,1,1,1,1,1,0}
        };

        enum DIR { EAST, SOUTH, WEST, NORTH };
        DIR direction = DIR.SOUTH;
        Point whereami = new Point(0, 0);

        Point fix_ul = new Point(36, 18);
        Point fix_ll = new Point(36, 218);
        Point fix_ur = new Point(336, 18);
        Point fix_lr = new Point(336, 218);

        Point var_ul = new Point(136, 98);
        Point var_ll = new Point(136, 148);
        Point var_ur = new Point(236, 98);
        Point var_lr = new Point(236, 148);

        eyeview mainview = new eyeview();

        int getMapInfo(Point point)
        {
            if (point.X > (map_x - 1) || point.Y > (map_y-1)) return 0;
            if (point.X < 0 || point.Y < 0) return 0;
            return map[point.X, point.Y];
        }

        int[,] getPathInfo(DIR dir, Point p)
        {
            int[,] status = new int[3,2]{{0,0},{0,0},{0,0}};
            int t;
            Point temp_p = p;
            int count_forward = 0;

            for (int i = 0, j = 0; i < 4; i++)
            {
                switch (dir)
                {
                    case DIR.SOUTH:
                        temp_p.X = p.X + i;
                        break;
                    case DIR.NORTH:
                        temp_p.X = p.X - i;
                        break;
                    case DIR.EAST:
                        temp_p.Y = p.Y + i;
                        break;
                    case DIR.WEST:
                        temp_p.Y = p.Y - i;
                        break;
                    
                }

                //Console.WriteLine("p="+temp_p);

                if (getMapInfo(temp_p) == 0)
                    break;

                t = getLeftRightInfo(dir, temp_p);
                if ( t > 0)
                {
                    status[j, 0] = i;
                    status[j++, 1] = t;
                }

                count_forward++;
            }

            if (count_forward == 4)
            {
                temp_p = p;

                switch (dir)
                {
                    case DIR.SOUTH:
                        temp_p.X = p.X + 4;
                        break;
                    case DIR.NORTH:
                        temp_p.X = p.X - 4;
                        break;
                    case DIR.EAST:
                        temp_p.Y = p.Y + 4;
                        break;
                    case DIR.WEST:
                        temp_p.Y = p.Y - 4;
                        break;
                }

                if (getMapInfo(temp_p) == 1)
                {
                    count_forward++;
                    //Console.WriteLine("forward + 1");
                }
            }

            status[2, 0] = count_forward;
            status[2, 1] = 4;

            //for (int i = 0; i < 3; i++ )
            //    Console.WriteLine("status=" + status[i,0] + status[i,1]);
            return status;
        }

        //Left:1 Right:2 Both:3 Forward:4
        int getLeftRightInfo(DIR dir, Point p)
        {
            int status = 0;

            switch (dir)
            {
                case DIR.SOUTH:
                    if (p.Y + 1 < map_y && map[p.X, p.Y + 1] == 1)
                        status = 1;//EAST, LEFT
                    if (p.Y-1 >=0 && map[p.X, p.Y - 1] == 1)
                        status += 2;//WEST, RIGHT
                    break;
                case DIR.NORTH:
                    if (p.Y + 1 < map_y && map[p.X, p.Y + 1] == 1)
                        status = 2;//EAST, RIGHT
                    if (p.Y - 1 >= 0 && map[p.X, p.Y - 1] == 1)
                        status += 1;//WEST, LEFT
                    break;
                case DIR.EAST:
                    if (p.X + 1 < map_x && map[p.X + 1, p.Y] == 1)
                        status = 2;//SOUTH, RIGHT
                    if (p.X-1>=0 && map[p.X-1, p.Y] == 1)
                        status += 1;//NORTH, LEFT
                    break;
                case DIR.WEST:
                    if (p.X + 1 < map_x && map[p.X + 1, p.Y] == 1)
                        status = 1;//SOUTH, LEFT
                    if (p.X - 1 >= 0 && map[p.X - 1, p.Y] == 1)
                        status += 2;//NORTH, RIGHT
                    break;
            }
            return status;
        }

        void createMainView(int[,] view, eyeview mainview)
        {
            int left_view = 10;
            int right_view = 10;
            int forward_steps = 0;
            int one_or_two_branches = 0;
            bool left_view_two_walls;
            bool right_view_two_walls;
            Brush color;

            eyeview.Composition leftview_walls = eyeview.Composition.BY_THREE;
            eyeview.Composition rightview_walls = eyeview.Composition.BY_THREE;

            for (int i = 0; i < 3; i++)
                if (view[i, 1] > 0)
                {
                    switch (view[i, 1])
                    {
                        case 1:
                            left_view = view[i, 0];
                            one_or_two_branches += view[i, 1];
                            break;
                        case 2:
                            right_view = view[i, 0];
                            one_or_two_branches += view[i, 1];
                            break;
                        case 3:
                            left_view = right_view = view[i, 0];
                            break;
                        case 4:
                            forward_steps = view[i, 0];
                            break;
                    }
                }
            
            //Console.WriteLine("view=" + left_view + right_view);

            // only one side branch
            left_view_two_walls = right_view_two_walls = (one_or_two_branches < 3 && one_or_two_branches > 0);
            //Console.WriteLine("bool1=" + left_view_two_walls);
            // left side branch position is closer than right side branch
            left_view_two_walls |= (left_view > right_view);
            //Console.WriteLine("bool2=" + left_view_two_walls);
            // left side branch position is the same with right side branch 
            left_view_two_walls |= (left_view == right_view);
            //Console.WriteLine("bool4=" + left_view_two_walls);
            // start point or stop point
            left_view_two_walls &= (forward_steps - 1) == left_view;
            //Console.WriteLine("bool3=" + left_view_two_walls + " " + left_view + " " + forward_steps);
            leftview_walls = left_view_two_walls ? eyeview.Composition.BY_TWO : eyeview.Composition.BY_THREE;


            // right side branch position is closer than left side branch
            right_view_two_walls |= (left_view < right_view);
            // right side branch position is the same with left side branch 
            right_view_two_walls |= (left_view == right_view);
            //Console.WriteLine("bool4=" + right_view_two_walls);
            // start point or stop point
            right_view_two_walls &= (forward_steps - 1) == right_view;
            //Console.WriteLine("bool5=" + right_view_two_walls);
            rightview_walls = right_view_two_walls ? eyeview.Composition.BY_TWO : eyeview.Composition.BY_THREE;

            mainview.clearView(walls);
            if (forward_steps == 5)
                color = mainview.setMiddleView(5, ref var_ll, ref var_lr, ref var_ul, ref var_ur);
            else
                color = mainview.setMiddleView(4 - forward_steps, ref var_ll, ref var_lr, ref var_ul, ref var_ur);
            //Console.WriteLine("forward_steps=" + forward_steps);
            //Console.WriteLine("middle point={0} {1} {2} {3}", var_ll, var_lr, var_ul, var_ur);
            label1.Text = direction + " " + whereami;
            if (left_view == 10)
                mainview.leftnoBranchView(walls, fix_ul, var_ul, var_ll, fix_ll);
            else
                mainview.leftBranchView(3 - left_view, leftview_walls, walls, var_ul, var_ll);
            mainview.middleView(walls, var_ul, var_ur, var_lr, var_ll, color);
            
            if (right_view == 10)
                mainview.rightnoBranchView(walls, var_ur, fix_ur, fix_lr, var_lr);
            else
                mainview.rightBranchView(3 - right_view, rightview_walls, walls, var_ur, var_lr);
        }

       
        public Form1()
        {
            InitializeComponent();

            int[,] view = getPathInfo(direction, whereami);
            createMainView(view, mainview); 
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            GraphicsPath panelPath = new GraphicsPath();

            foreach(var wall in walls)
            {
                panelPath.AddPolygon(wall.getWallPoints());
                e.Graphics.FillPath(wall.getWallColor(), panelPath);
                e.Graphics.DrawPath(blackPen, panelPath);

                panelPath.Reset();
            }

            //ceiling
            panelPath.AddPolygon(mainview.traceCeilingpoints(walls));
            e.Graphics.FillPath(mainview.CeilingBrush, panelPath);
            panelPath.Reset();

            //floor
            panelPath.AddPolygon(mainview.traceFloorpoints(walls));
            e.Graphics.FillPath(mainview.FloorBrush, panelPath);
          

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int[,] view;

            switch(e.KeyCode)
            {
                case Keys.Down:

                    switch(direction)
                    {
                        case DIR.SOUTH:
                             whereami.X -= 1;
                            if (whereami.X < 0 || getMapInfo(whereami) == 0)
                                whereami.X += 1;
                            break;
                        case DIR.NORTH:
                            whereami.X += 1;
                            if (whereami.X > (map_x-1) || getMapInfo(whereami) == 0)
                                whereami.X -= 1;
                            break;
                        case DIR.WEST:
                            whereami.Y += 1;
                            if (whereami.Y > (map_y-1) || getMapInfo(whereami) == 0)
                                whereami.Y -= 1;
                            break;
                        case DIR.EAST:
                            whereami.Y -= 1;
                            if (whereami.Y < 0 || getMapInfo(whereami) == 0)
                                whereami.Y += 1;
                            break;
                    }

                    Console.WriteLine("Down key" + whereami);
                    
                   
                    break;
                case Keys.Up:

                    switch(direction)
                    {
                        case DIR.SOUTH:
                            whereami.X += 1;
                            if (whereami.X > (map_x-1) || getMapInfo(whereami) == 0)
                                whereami.X -= 1;
                            break;
                        case DIR.NORTH:
                            whereami.X -= 1;
                            if (whereami.X < 0 || getMapInfo(whereami) == 0)
                                whereami.X += 1; 
                            break;
                        case DIR.EAST:
                             whereami.Y += 1;
                             if (whereami.Y > (map_y-1) || getMapInfo(whereami) == 0)
                                whereami.Y -= 1;
                            break;
                        case DIR.WEST:
                            whereami.Y -= 1;
                            if (whereami.Y < 0 || getMapInfo(whereami) == 0)
                                whereami.Y += 1;
                            break;
                    }
                   
                    Console.WriteLine("Up key" + whereami);
                    
                    
                    break;
                case Keys.Left:
                    switch (direction)
                    {
                        case DIR.SOUTH:
                            direction = DIR.EAST;
                            break;
                        case DIR.NORTH:
                            direction = DIR.WEST;
                            break;
                        case DIR.EAST:
                            direction = DIR.NORTH;
                            break;
                        case DIR.WEST:
                            direction = DIR.SOUTH;
                            break;
                    }
                    Console.WriteLine("Left key" + direction);
                   
                    break;
                case Keys.Right:
                    switch (direction)
                    {
                        case DIR.SOUTH:
                            direction = DIR.WEST;
                            break;
                        case DIR.NORTH:
                            direction = DIR.EAST;
                            break;
                        case DIR.EAST:
                            direction = DIR.SOUTH;
                            break;
                        case DIR.WEST:
                            direction = DIR.NORTH;
                            break;
                    }
                    Console.WriteLine("Right key" + direction);
                    
                    break;
            }
            
            view = getPathInfo(direction, whereami);
            createMainView(view, mainview);
            this.panel1.Refresh();
        }


       
    }
}


public class wall
{
    Point upper_left, upper_right, lower_left, lower_right;
    Brush color;

    public wall(Point ul, Point ur, Point lr, Point ll, Brush color)
    {
        this.upper_left = ul;
        this.upper_right = ur;
        this.lower_right = lr;
        this.lower_left = ll;
        this.color = color;
    }

    public override string ToString()
    {
        return "upper left= " + this.upper_left.ToString() + "\n" +
               "upper right= " + this.upper_right.ToString() + "\n" +
               "lower left= " + this.lower_left.ToString() + "\n" +
               "lower right= " + this.lower_right.ToString();
    }

    public Point[] getCeilingPoints()
    {
        Point[] ceilingpoints = new Point[2];

        ceilingpoints[0] = this.upper_left;
        ceilingpoints[1] = this.upper_right;
        return ceilingpoints;
    }

    public Point[] getFloorPoints()
    {
        Point[] floorpoints = new Point[2];

        floorpoints[0] = this.lower_left;
        floorpoints[1] = this.lower_right;
        return floorpoints;
    }

    public Point[] getWallPoints()
    {
        Point[] wallpoints = new Point[4];

        wallpoints[0] = this.upper_left;
        wallpoints[1] = this.upper_right;
        wallpoints[2] = this.lower_right;
        wallpoints[3] = this.lower_left;
        return wallpoints;
    }

    public Brush getWallColor()
    {
        return color;
    }
}


public class eyeview
{
    public LinearGradientBrush leftWallBrush = new LinearGradientBrush(
                                        new Point(0, 10),
                                        new Point(200, 10),
                                        Color.Gray,
                                        Color.Black);

    public LinearGradientBrush rightWallBrush = new LinearGradientBrush(
                                        new Point(0, 10),
                                        new Point(200, 10),
                                        Color.Black,
                                        Color.Gray);

    public LinearGradientBrush CeilingBrush = new LinearGradientBrush(
                                        new Point(0, 0),
                                        new Point(0, 100),
                                        Color.Aqua,
                                        Color.Black);

    public LinearGradientBrush FloorBrush = new LinearGradientBrush(
                                        new Point(0, 0),
                                        new Point(0, 110),
                                        Color.Black,
                                        Color.Red);

    public enum Composition { BY_TWO, BY_THREE };

    public void clearView(List<wall> lists)
    {
        lists.Clear();
    }

    public void leftnoBranchView(List<wall> lists, Point x, Point y, Point z, Point w)
    {
        lists.Add(new wall(x, y, z, w, leftWallBrush));
    }

    public void rightnoBranchView(List<wall> lists, Point x, Point y, Point z, Point w)
    {
        lists.Add(new wall(x, y, z, w, rightWallBrush));
    }

    public void leftBranchView(int state, Composition n_walls, List<wall> lists, Point x, Point y)
    {
        int p1 = 0;
        int p2 = 0;
        int p3 = 0;
        Brush color = Brushes.Black;

        switch (state)
        {
            case 3:
                p1 = 36 + 0;
                p2 = 36 + 20;
                p3 = 36 + 20;
                color = Brushes.LightGray;
                break;
            case 2:
                p1 = 36 + 10;
                p2 = 36 + 10 + 40;
                p3 = 36 + 10 + 40;
                color = Brushes.DarkGray;
                
                break;
            case 1:
                p1 = 36 + 35;
                p2 = 36 + 35 + 35;
                p3 = 36 + 35 + 35;
                color = Brushes.Gray;
                break;
            case 0:
                p1 = 36 + 80;
                p2 = 36 + 80 + 10;
                p3 = 36 + 80 + 10;
                color = Brushes.DimGray;
                break;
        }
        lists.Add(new wall(new Point(36, 18), new Point(p1, getLeftUpperCoordY(p1)), new Point(p1, getLeftLowerCoordY(p1)), new Point(36, 218), leftWallBrush));

        if (n_walls == Composition.BY_TWO)
        {
            lists.Add(new wall(new Point(p1, x.Y), x, y, new Point(p1, y.Y), color));
        }
        else
        {
            lists.Add(new wall(new Point(p1, getLeftUpperCoordY(p3)), new Point(p3, getLeftUpperCoordY(p3)), new Point(p3, getLeftLowerCoordY(p3)), new Point(p1, getLeftLowerCoordY(p3)), Brushes.DarkGray));
            lists.Add(new wall(new Point(p3, getLeftUpperCoordY(p3)), x, y, new Point(p3, getLeftLowerCoordY(p3)), leftWallBrush));
        }
    }


    public Brush setMiddleView(int state, ref Point x, ref Point y, ref Point z, ref Point w)
    {
        int x1 = 0;
        int x2 = 0;
        Brush color = Brushes.Black;

        switch (state)
        {
            case 0:
                x1 = 136;
                x2 = 236;
                color = Brushes.DimGray;
                break;
            case 1:
                x1 = 136 - 20;
                x2 = 236 + 20;
                color = Brushes.Gray;
                break;
            case 2:
                x1 = 136 - 40;
                x2 = 236 + 40;
                color = Brushes.DarkGray;
                break;
            case 3:
                x1 = 136 - 70;
                x2 = 236 + 70;
                color = Brushes.LightGray;
                break;
            case 5:
                x1 = 136;
                x2 = 236;
                break;
        }

        x.X = x1;
        x.Y = getLeftLowerCoordY(x1);
        y.X = x2;
        y.Y = getRightLowerCoordY(x2);
        z.X = x1;
        z.Y = getLeftUpperCoordY(x1);
        w.X = x2;
        w.Y = getRightUpperCoordY(x2);
        
        return color;
    }

    public void middleView(List<wall> lists, Point x, Point y, Point z, Point w, Brush color)
    {
        lists.Add(new wall(x, y, z, w, color));
    }

    public void rightBranchView(int state, Composition n_walls, List<wall> lists, Point x, Point y)
    {
        int p1 = 0;
        int p2 = 0;
        int p3 = 0;
        Brush color = Brushes.Black;

        switch (state)
        {
            case 0:
                p1 = 236 + 10;
                p2 = 236 + 10 + 10;
                p3 = 236 + 10 + 10;
                color = Brushes.DimGray;
                break;
            case 1:
                p1 = 236 + 30;
                p2 = 236 + 30 + 35;
                p3 = 236 + 30 + 35;
                color = Brushes.Gray;
                break;
            case 2:
                p1 = 236 + 50;
                p2 = 236 + 50 + 40;
                p3 = 236 + 50 + 40;
                color = Brushes.DarkGray;
                break;
            case 3:
                p1 = 236 + 80;
                p2 = 236 + 80 + 20;
                p3 = 236 + 80 + 20;
                color = Brushes.LightGray;
                break;
        }

        if (n_walls == Composition.BY_TWO)
        {
            lists.Add(new wall(x, new Point(p3, x.Y), new Point(p3, y.Y), y, color));
        }
        else
        {
            lists.Add(new wall(x, new Point(p1, getRightUpperCoordY(p1)), new Point(p1, getRightLowerCoordY(p1)), y, rightWallBrush));
            lists.Add(new wall(new Point(p1, getRightUpperCoordY(p1)), new Point(p2, getRightUpperCoordY(p1)), new Point(p2, getRightLowerCoordY(p1)), new Point(p1, getRightLowerCoordY(p1)), Brushes.Gray));
        }

        lists.Add(new wall(new Point(p3, getRightUpperCoordY(p3)), new Point(336, 18), new Point(336, 218), new Point(p3, getRightLowerCoordY(p3)), rightWallBrush));
    }

    public Point[] traceCeilingpoints(List<wall> lists)
    {
        Point[] all = new Point[lists.Count * 2];

        for (int i = 0; i < lists.Count; i++)
        {
            Point[] p = lists[i].getCeilingPoints();
            for (int j = 0; j < 2; j++)
                all[2 * i + j] = p[j];
        }
        return all;
    }

    public Point[] traceFloorpoints(List<wall> lists)
    {
        Point[] all = new Point[lists.Count * 2];

        for (int i = 0; i < lists.Count; i++)
        {
            Point[] p = lists[i].getFloorPoints();
            for (int j = 0; j < 2; j++)
                all[2 * i + j] = p[j];
        }
        return all;
    }

    int getRightUpperCoordY(int x)
    {
        return (286 - 4 * x / 5);
    }

    int getRightLowerCoordY(int x)
    {
        return (7 * x / 10 - 17);
    }

    int getLeftUpperCoordY(int x)
    {
        return (4 * x / 5 - 10);
    }

    int getLeftLowerCoordY(int x)
    {
        return (243 - 7 * x / 10);
    }
}
