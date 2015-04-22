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

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        string major_version = "V1.1";

        Pen blackPen = new Pen(Color.Black, 1);
        Pen bluePen = new Pen(Color.Blue, 3);
        Pen grayPen = new Pen(Color.Gray, 3);

        List<wall> walls = new List<wall>();

        private const int map_x = 10;
        private const int map_y = 10;
        int level_index = 0;

        int[,,] map = new int[3, map_x, map_y]{
                                  
                                 {{1,0,2,0,0,0,1,1,1,0},
                                  {1,0,1,1,1,1,1,0,1,0},
                                  {1,0,0,0,0,0,0,1,1,0},
                                  {1,1,1,1,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,0,0},
                                  {0,0,0,0,1,0,0,1,1,0},
                                  {0,0,0,0,1,0,0,0,1,0},
                                  {0,0,0,0,1,1,1,1,1,0}},

                                 {{1,0,0,1,1,1,0,0,0,0},
                                  {1,1,1,1,0,1,1,1,0,0},
                                  {0,0,0,0,0,0,0,1,1,1},
                                  {1,1,1,0,1,0,0,0,0,1},
                                  {1,0,1,0,0,0,0,1,1,1},
                                  {1,0,1,1,1,1,0,1,0,0},
                                  {1,1,0,0,0,1,0,1,1,1},
                                  {0,1,1,0,1,1,0,0,0,1},
                                  {0,0,1,0,1,0,0,0,1,1},
                                  {2,1,1,0,1,1,1,1,1,0}},

                                 {{1,0,1,1,1,1,1,1,1,1},
                                  {1,0,1,0,0,0,0,0,0,1},
                                  {1,0,1,0,1,1,1,1,0,1},
                                  {1,0,1,0,1,0,0,1,0,1},
                                  {1,0,1,0,1,0,0,1,0,1},
                                  {1,0,1,0,1,1,0,1,0,1},
                                  {1,0,1,0,0,0,0,1,0,1},
                                  {1,0,1,1,1,1,1,1,0,1},
                                  {1,0,0,0,0,0,0,0,0,1},
                                  {1,1,1,1,1,1,1,1,1,1}}
        };

        enum DIR { EAST, SOUTH, WEST, NORTH };
        DIR direction = DIR.SOUTH;
        Point whereami = new Point(0, 0);

        eyeview mainview = new eyeview();

        bool level_up = false;
        int[] r_target = new int[3] { 0, 0, 0 };
        int[] r_curr = new int[3] { 236, 236, 236 };

        int[] l_target = new int[3] { 0, 0, 0 };
        int[] l_curr = new int[3] { 236, 236, 236 };

        int getMapInfo(Point point)
        {
            if (point.X > (map_x - 1) || point.Y > (map_y-1)) return 0;
            if (point.X < 0 || point.Y < 0) return 0;
            return map[level_index, point.X, point.Y];
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

            if (getMapInfo(p) == 2)
                count_forward = 6;

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
                    if (p.Y + 1 < map_y && map[level_index, p.X, p.Y + 1] > 0)
                        status = 1;//EAST, LEFT
                    if (p.Y - 1 >= 0 && map[level_index, p.X, p.Y - 1] > 0)
                        status += 2;//WEST, RIGHT
                    break;
                case DIR.NORTH:
                    if (p.Y + 1 < map_y && map[level_index, p.X, p.Y + 1] > 0)
                        status = 2;//EAST, RIGHT
                    if (p.Y - 1 >= 0 && map[level_index, p.X, p.Y - 1] > 0)
                        status += 1;//WEST, LEFT
                    break;
                case DIR.EAST:
                    if (p.X + 1 < map_x && map[level_index, p.X + 1, p.Y] > 0)
                        status = 2;//SOUTH, RIGHT
                    if (p.X - 1 >= 0 && map[level_index, p.X - 1, p.Y] > 0)
                        status += 1;//NORTH, LEFT
                    break;
                case DIR.WEST:
                    if (p.X + 1 < map_x && map[level_index, p.X + 1, p.Y] > 0)
                        status = 1;//SOUTH, LEFT
                    if (p.X - 1 >= 0 && map[level_index, p.X - 1, p.Y] > 0)
                        status += 2;//NORTH, RIGHT
                    break;
            }
            return status;
        }

        public void configLeftBranchView(int state)
        {
            switch (state)
            {
                case 0:
                    l_target[0] = 36 + 0;
                    l_target[1] = 36 + 20;
                    l_target[2] = 36 + 20;
                    break;
                case 1:
                    l_target[0] = 36 + 10;
                    l_target[1] = 36 + 10 + 40;
                    l_target[2] = 36 + 10 + 40;
                    break;
                case 2:
                    l_target[0] = 36 + 35;
                    l_target[1] = 36 + 35 + 35;
                    l_target[2] = 36 + 35 + 35;
                    break;
                case 3:
                    l_target[0] = 36 + 80;
                    l_target[1] = 36 + 80 + 10;
                    l_target[2] = 36 + 80 + 10;
                    break;
            }
        }




        void configRightBranchView(int state)
        {
            switch (state)
            {
                case 3:
                    r_target[0] = 236 + 10;
                    r_target[1] = 236 + 10 + 20;
                    r_target[2] = 236 + 10 + 20;
                    break;
                case 2:
                    r_target[0] = 236 + 30;
                    r_target[1] = 236 + 30 + 35;
                    r_target[2] = 236 + 30 + 35;
                    break;
                case 1:
                    r_target[0] = 236 + 50;
                    r_target[1] = 236 + 50 + 40;
                    r_target[2] = 236 + 50 + 40;
                    break;
                case 0:
                    r_target[0] = 236 + 80;
                    r_target[1] = 236 + 80 + 20;
                    r_target[2] = 236 + 80 + 20;
                    break;
            }
        }

        void changeRightBranchView()
        {
            if (r_curr[0] != r_target[0])
            {
                r_curr[0] += 5;
                r_curr[1] += 5;
                r_curr[2] += 5;
            }
            else
                timer1.Enabled = false;
        }

        delegate void viewHandler(List<wall> lists, int[] p);
        viewHandler rightviewhandler;
        viewHandler leftviewhandler;
        viewHandler middleviewhandler;


        int left_view;
        int right_view;
        int forward_steps = 0;
        eyeview.Composition leftview_walls = eyeview.Composition.BY_THREE;
        eyeview.Composition rightview_walls = eyeview.Composition.BY_THREE;

        void getMainViewFormInfo(int[,] view)
        {
            left_view = 10;
            right_view = 10;

            for (int i = 0; i < 3; i++)
                if (view[i, 1] > 0)
                {
                    switch (view[i, 1])
                    {
                        case 1:
                            left_view = view[i, 0];
                            break;
                        case 2:
                            right_view = view[i, 0];
                            break;
                        case 3:
                            left_view = right_view = view[i, 0];
                            break;
                        case 4:
                            forward_steps = view[i, 0];
                            break;
                    }
                }
        }

        void configMainViewForm()
        {
            bool left_view_two_walls;
            bool right_view_two_walls;
            
            //Console.WriteLine("view=" + left_view + right_view);

            // only one side branch
            left_view_two_walls = right_view_two_walls = (left_view == 10 || right_view == 10);
            // left side branch position is closer than right side branch
            left_view_two_walls |= (left_view > right_view);
            // left side branch position is the same with right side branch 
            left_view_two_walls |= (left_view == right_view);
            // start point or stop point
            left_view_two_walls &= (forward_steps - 1) == left_view;
            leftview_walls = left_view_two_walls ? eyeview.Composition.BY_TWO : eyeview.Composition.BY_THREE;

            // right side branch position is closer than left side branch
            right_view_two_walls |= (left_view < right_view);
            // right side branch position is the same with left side branch 
            right_view_two_walls |= (left_view == right_view);
            // start point or stop point
            right_view_two_walls &= (forward_steps - 1) == right_view;
            rightview_walls = right_view_two_walls ? eyeview.Composition.BY_TWO : eyeview.Composition.BY_THREE;

            //Console.WriteLine("bool=" + left_view + " "
            //                          + right_view + " "
            //                          + forward_steps);
            mainview.configMiddleView(forward_steps);
            mainview.setMiddleViewColor(forward_steps);

            configLeftBranchView(left_view);
            mainview.setleftBranchViewColor(left_view);

            mainview.setrightBranchViewColor(right_view);
            configRightBranchView(right_view);
        }

        void createMainViewForm()
        {
            mainview.clearView(walls);

            label1.Text = direction + " " + whereami;
            if (left_view == 10)
                mainview.leftnoBranchView(walls, l_target);
            else
            {
                if (leftview_walls == eyeview.Composition.BY_TWO)
                    mainview.setLeftBranchViewTwo(walls, l_target);
                else
                    mainview.setLeftBranchViewThree(walls, l_target);
            }

            mainview.setMiddleView(walls, l_target);

            if (right_view == 10)
                mainview.rightnoBranchView(walls, r_target);
            else
            {
                if (rightview_walls == eyeview.Composition.BY_TWO)
                    mainview.setrightBranchViewTwo(walls, r_target);
                else
                    mainview.setrightBranchViewThree(walls, r_target);
            }
        }


        public Form1()
        {
            InitializeComponent();

            label2.Text = major_version;
            label3.Text = "Level " + level_index.ToString();

            int[,] view = getPathInfo(direction, whereami);
            getMainViewFormInfo(view);
            configMainViewForm();
            createMainViewForm();
        }

        private void DrawStringPointF(PaintEventArgs e, String drawString)
        {
            // Create font and brush.
            Font drawFont = new Font("Arial", 28);
            SolidBrush drawBrush = new SolidBrush(Color.Red);

            // Create point for upper-left corner of drawing.
            PointF drawPoint = new PointF(130.0F, 108.0F);

            // Draw string to screen.
            e.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            GraphicsPath panelPath = new GraphicsPath();

            if (walls.Count != 0)
            {
                foreach (var wall in walls)
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

            //DrawStringPointF(e, "Level2");

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
                    level_up = false;
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

                    if (getMapInfo(whereami) == 2)
                    {
                        switch (level_up)
                        {
                            case false:
                                level_up = true;
                                break;
                            case true:
                                level_index++;
                                whereami.X = 0;
                                whereami.Y = 0;
                                direction = DIR.SOUTH;
                                level_up = false;
                                label3.Text = "Level " + level_index.ToString();
                                break;
                        }
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
            getMainViewFormInfo(view);
            configMainViewForm();
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            createMainViewForm();
            this.panel1.Refresh();
            timer1.Enabled = false;
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

    public LinearGradientBrush LevelBrush = new LinearGradientBrush(
                                        new Point(0, 0),
                                        new Point(0, 200),
                                        Color.Aqua,
                                        Color.Red);

    public enum Composition { BY_TWO, BY_THREE };
    Brush rightside_color, middle_color,leftside_color;

    Point fix_ul = new Point(36, 18);
    Point fix_ll = new Point(36, 218);
    Point fix_ur = new Point(336, 18);
    Point fix_lr = new Point(336, 218);

    Point var_ul = new Point(136, 98);
    Point var_ll = new Point(136, 148);
    Point var_ur = new Point(236, 98);
    Point var_lr = new Point(236, 148);

    public void clearView(List<wall> lists)
    {
        lists.Clear();
    }

    public void leftnoBranchView(List<wall> lists, int[] p)
    {
        lists.Add(new wall(fix_ul, var_ul, var_ll, fix_ll, leftWallBrush));
    }
    
    public void rightnoBranchView(List<wall> lists, int[] p)
    {
        lists.Add(new wall(var_ur, fix_ur, fix_lr, var_lr, rightWallBrush));
    }

    public void setleftBranchViewColor(int state)
    {
        switch (state)
        {
            case 0:
                leftside_color = Brushes.LightGray;
                break;
            case 1:
                leftside_color = Brushes.DarkGray;
                break;
            case 2:
                leftside_color = Brushes.Gray;
                break;
            case 3:
                leftside_color = Brushes.DimGray;
                break;
        }
    }

    public void setLeftBranchViewTwo(List<wall> lists, int[] p)
    {
        lists.Add(new wall(new Point(36, 18), new Point(p[0], getLeftUpperCoordY(p[0])), new Point(p[0], getLeftLowerCoordY(p[0])), new Point(36, 218), leftWallBrush));
        lists.Add(new wall(new Point(p[0], var_ul.Y), var_ul, var_ll, new Point(p[0], var_ll.Y), leftside_color));
    }

    public void setLeftBranchViewThree(List<wall> lists, int[] p)
    {
        lists.Add(new wall(new Point(36, 18), new Point(p[0], getLeftUpperCoordY(p[0])), new Point(p[0], getLeftLowerCoordY(p[0])), new Point(36, 218), leftWallBrush));
        lists.Add(new wall(new Point(p[0], getLeftUpperCoordY(p[2])), new Point(p[2], getLeftUpperCoordY(p[2])), new Point(p[2], getLeftLowerCoordY(p[2])), new Point(p[0], getLeftLowerCoordY(p[2])), Brushes.DarkGray));
        lists.Add(new wall(new Point(p[2], getLeftUpperCoordY(p[2])), var_ul, var_ll, new Point(p[2], getLeftLowerCoordY(p[2])), leftWallBrush));
    }

    public void setMiddleViewColor(int state)
    {
        switch (state)
        {
            case 4:
                middle_color = Brushes.DimGray;
                break;
            case 3:
                middle_color = Brushes.Gray;
                break;
            case 2:
                middle_color = Brushes.DarkGray;
                break;
            case 1:
                middle_color = Brushes.LightGray;
                break;
            case 5:
                middle_color = Brushes.Black;
                break;
            case 6:
                middle_color = LevelBrush;
                break;
        }
    }


    public void configMiddleView(int state)
    {
        int x1 = 0;
        int x2 = 0;

        switch (state)
        {
            case 4:
                x1 = 136;
                x2 = 236;
                break;
            case 3:
                x1 = 136 - 20;
                x2 = 236 + 20;
                break;
            case 2:
                x1 = 136 - 40;
                x2 = 236 + 40;
                break;
            case 1:
                x1 = 136 - 70;
                x2 = 236 + 70;
                break;
            case 5:
                x1 = 136;
                x2 = 236;
                break;
            case 6:
                x1 = 136 - 70;
                x2 = 236 + 70;
                break;
        }

        var_ll.X = x1;
        var_ll.Y = getLeftLowerCoordY(x1);
        var_lr.X = x2;
        var_lr.Y = getRightLowerCoordY(x2);
        var_ul.X = x1;
        var_ul.Y = getLeftUpperCoordY(x1);
        var_ur.X = x2;
        var_ur.Y = getRightUpperCoordY(x2);
    }


    public void setMiddleView(List<wall> lists, int[] p)
    {
        lists.Add(new wall(var_ul, var_ur, var_lr, var_ll, middle_color));
    }

    public void setrightBranchViewColor(int state)
    {
        switch (state)
        {
            case 3:
                rightside_color = Brushes.DimGray;
                break;
            case 2:
                rightside_color = Brushes.Gray;
                break;
            case 1:
                rightside_color = Brushes.DarkGray;
                break;
            case 0:
                rightside_color = Brushes.LightGray;
                break;
        }
    }

    public void setrightBranchViewTwo(List<wall> lists, int[] p)
    {
        lists.Add(new wall(var_ur, new Point(p[2], var_ur.Y), new Point(p[2], var_lr.Y), var_lr, rightside_color));
        lists.Add(new wall(new Point(p[2], getRightUpperCoordY(p[2])), new Point(336, 18), new Point(336, 218), new Point(p[2], getRightLowerCoordY(p[2])), rightWallBrush));
    }

    public void setrightBranchViewThree(List<wall> lists, int[] p)
    {
        lists.Add(new wall(var_ur, new Point(p[0], getRightUpperCoordY(p[0])), new Point(p[0], getRightLowerCoordY(p[0])), var_lr, rightWallBrush));
        lists.Add(new wall(new Point(p[0], getRightUpperCoordY(p[0])), new Point(p[1], getRightUpperCoordY(p[0])), new Point(p[1], getRightLowerCoordY(p[0])), new Point(p[0], getRightLowerCoordY(p[0])), Brushes.Gray));
        lists.Add(new wall(new Point(p[2], getRightUpperCoordY(p[2])), new Point(336, 18), new Point(336, 218), new Point(p[2], getRightLowerCoordY(p[2])), rightWallBrush));
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

    public int getRightUpperCoordY(int x)
    {
        return (286 - 4 * x / 5);
    }

    public int getRightLowerCoordY(int x)
    {
        return (7 * x / 10 - 17);
    }

    public int getLeftUpperCoordY(int x)
    {
        return (4 * x / 5 - 10);
    }

    public int getLeftLowerCoordY(int x)
    {
        return (243 - 7 * x / 10);
    }
}
