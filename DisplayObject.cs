using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ootpisp
{
    public struct PointD
    {
        public double X { get; set; }
        public double Y { get; set; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public struct RectangleD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public RectangleD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public double Left => X;
        public double Right => X + Width;
        public double Top => Y;
        public double Bottom => Y + Height;
    }

    public abstract class DisplayObject
    {
        protected PointD position;
        protected PointD velocity;
        protected PointD acceleration;
        protected Color fillColor;
        protected double size;
        protected double mass;
        protected double borderThickness;
        protected Pen borderPen;
        protected Color borderColor;
        protected bool isAccelerating = false;
        private static readonly Random rand = new Random();
        protected bool isMoving = true;

        [JsonIgnore]
        public virtual RectangleD Bounds
        {
            get
            {
                return new RectangleD(
                    position.X - size,
                    position.Y - size,
                    size * 2,
                    size * 2
                );
            }
        }

        [JsonProperty("PositionX")]
        public double PositionX => position.X;
        [JsonProperty("PositionY")]
        public double PositionY => position.Y;
        [JsonProperty("VelocityX")]
        public double VelocityX => velocity.X;
        [JsonProperty("VelocityY")]
        public double VelocityY => velocity.Y;
        [JsonProperty("AccelerationX")]
        public double AccelerationX => acceleration.X;
        [JsonProperty("AccelerationY")]
        public double AccelerationY => acceleration.Y;
        [JsonProperty("ColorR")]
        public int ColorR => fillColor.R;
        [JsonProperty("ColorG")]
        public int ColorG => fillColor.G;
        [JsonProperty("ColorB")]
        public int ColorB => fillColor.B;
        [JsonProperty("BorderColorR")]
        public int BorderColorR => borderColor.R;
        [JsonProperty("BorderColorG")]
        public int BorderColorG => borderColor.G;
        [JsonProperty("BorderColorB")]
        public int BorderColorB => borderColor.B;
        [JsonProperty("BorderThickness")]
        public double BorderThickness => borderThickness;
        [JsonProperty("Size")]
        public double Size => size;
        [JsonProperty("IsMoving")]
        public bool IsMoving => isMoving;
        [JsonProperty("IsAccelerating")]
        public bool IsAccelerating => isAccelerating;
        [JsonProperty("Type")]
        public string Type => GetType().Name;
        [JsonProperty("Mass")]
        public double Mass => mass;

        public DisplayObject(double x, double y)
        {
            position = new PointD(x, y);
            double speed = rand.NextDouble() * 7 + 1;
            double angle = rand.NextDouble() * 2 * Math.PI;
            velocity = new PointD(
                speed * Math.Cos(angle),
                speed * Math.Sin(angle)
            );
            acceleration = new PointD(0, 0);
            fillColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            borderThickness = rand.NextDouble() * 3 + 1;
            borderColor = Color.FromArgb(0, 0, 0);
            borderPen = new Pen(borderColor, (float)borderThickness);
            size = rand.NextDouble() * 30 + 10;
            mass = 2;
        }

        [JsonConstructor]
        public DisplayObject(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        {
            position = new PointD(positionX, positionY);
            velocity = new PointD(velocityX, velocityY);
            acceleration = new PointD(accelerationX, accelerationY);
            fillColor = Color.FromArgb(colorR, colorG, colorB);
            this.borderThickness = borderThickness;
            borderColor = Color.FromArgb(borderColorR, borderColorG, borderColorB);
            borderPen = new Pen(borderColor, (float)borderThickness);
            this.size = size;
            this.isAccelerating = isAccelerating;
            this.isMoving = isMoving;
            this.mass = mass;
        }

        public void ToggleAcceleration()
        {
            double accelMagnitude = rand.NextDouble() * 0.5 - 0.1;
            double accelAngle = rand.NextDouble() * 2 * Math.PI;
            acceleration = new PointD(
                accelMagnitude * Math.Cos(accelAngle),
                accelMagnitude * Math.Sin(accelAngle)
            );
            isAccelerating = true;
        }

        public void DisableAcceleration()
        {
            acceleration = new PointD(0, 0);
            isAccelerating = false;
        }

        public virtual void Move(Rectangle bounds, DisplayObject[] others, Stats stats)
        {
            if (!isMoving) return;
            if (isAccelerating)
            {
                velocity.X += acceleration.X;
                velocity.Y += acceleration.Y;
            }
            position.X += velocity.X;
            position.Y += velocity.Y;

            // Отскок от стен
            RectangleD objBounds = Bounds;
            if (objBounds.Left < bounds.Left)
            {
                position.X = bounds.Left + size;
                velocity.X = -velocity.X;
            }
            if (objBounds.Right > bounds.Right)
            {
                position.X = bounds.Right - size;
                velocity.X = -velocity.X;
            }
            if (objBounds.Top < bounds.Top)
            {
                position.Y = bounds.Top + size;
                velocity.Y = -velocity.Y;
            }
            if (objBounds.Bottom > bounds.Bottom)
            {
                if (this is SpecialCircle)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        if (others[i] == this)
                        {
                            others[i] = null;
                            break;
                        }
                    }
                    return;
                }
                else if (this is PointObject)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        if (others[i] == this)
                        {
                            others[i] = null;
                            break;
                        }
                    }
                    return;
                }
                position.Y = bounds.Bottom - size;
                velocity.Y = -velocity.Y;
            }

            foreach (var other in others)
            {
                if (other == this || other == null) continue;

                if (other is Paddle paddle && !(this is PointObject))
                {
                    RectangleD paddleBounds = paddle.Bounds;
                    RectangleD ballBounds = this.Bounds;

                    if (ballBounds.Right > paddleBounds.Left && ballBounds.Left < paddleBounds.Right &&
                        ballBounds.Bottom > paddleBounds.Top && ballBounds.Top < paddleBounds.Bottom)
                    {
                        double dx = position.X - (paddleBounds.X + paddleBounds.Width / 2);
                        double dy = position.Y - (paddleBounds.Y + paddleBounds.Height / 2);
                        double absDx = Math.Abs(dx);
                        double absDy = Math.Abs(dy);

                        double minX = paddleBounds.Width / 2 + size;
                        double minY = paddleBounds.Height / 2 + size;
                        double overlapX = minX - absDx;
                        double overlapY = minY - absDy;

                        if (overlapX > 0 && overlapY > 0)
                        {
                            if (overlapX < overlapY)
                            {
                                if (dx > 0)
                                {
                                    position.X = paddleBounds.Right + size;
                                    velocity.X = Math.Abs(velocity.X);
                                }
                                else
                                {
                                    position.X = paddleBounds.Left - size;
                                    velocity.X = -Math.Abs(velocity.X);
                                }
                            }
                            else
                            {
                                if (dy > 0)
                                {
                                    position.Y = paddleBounds.Bottom + size;
                                    velocity.Y = Math.Abs(velocity.Y);
                                }
                                else
                                {
                                    position.Y = paddleBounds.Top - size;
                                    velocity.Y = -Math.Abs(velocity.Y);
                                    double hitPoint = (dx / (paddleBounds.Width / 2)) * 0.5;
                                    velocity.X += hitPoint * 5;
                                }
                            }
                        }
                    }
                }
                else if (!(this is PointObject) && !(other is PointObject))
                {
                    double dx = position.X - other.position.X;
                    double dy = position.Y - other.position.Y;
                    double distanceSquared = dx * dx + dy * dy;
                    double minDistance = size + other.size;
                    double distance = Math.Sqrt(distanceSquared);

                    if (distanceSquared < minDistance * minDistance && distance > 0) // Столкновение произошло
                    {
                        if (this is SpecialCircle && !(other is Paddle))
                        {
                            for (int i = 0; i < others.Length; i++)
                            {
                                if (others[i] == other)
                                {
                                    stats.AddScore(100);
                                    for (int j = 0; j < others.Length; j++)
                                    {
                                        if (others[j] == null)
                                        {
                                            others[j] = new PointObject(other.position.X, other.position.Y);
                                            break;
                                        }
                                    }
                                    others[i] = null;
                                    break;
                                }
                            }
                        }
                        else if (other is SpecialCircle && !(this is Paddle))
                        {
                            for (int i = 0; i < others.Length; i++)
                            {
                                if (others[i] == this)
                                {
                                    stats.AddScore(100);
                                    for (int j = 0; j < others.Length; j++)
                                    {
                                        if (others[j] == null)
                                        {
                                            others[j] = new PointObject(position.X, position.Y);
                                            break;
                                        }
                                    }
                                    others[i] = null;
                                    break;
                                }
                            }
                        }
                        double nx = dx / distance; // Нормализованный вектор направления
                        double ny = dy / distance;

                        double relativeVelocityX = velocity.X - other.velocity.X;
                        double relativeVelocityY = velocity.Y - other.velocity.Y;

                        double normalSpeed = relativeVelocityX * nx + relativeVelocityY * ny;
                        if (normalSpeed >= 0) return;

                        double impulse = (2 * normalSpeed * mass * other.mass) / (mass + other.mass);

                        velocity.X -= (impulse / mass) * nx;
                        velocity.Y -= (impulse / mass) * ny;
                        other.velocity.X += (impulse / other.mass) * nx;
                        other.velocity.Y += (impulse / other.mass) * ny;

                        // Сдвигаем, чтобы не залипли
                        double overlap = minDistance - distance;
                        double correction = overlap / 2;
                        position.X += nx * correction;
                        position.Y += ny * correction;
                        other.position.X -= nx * correction;
                        other.position.Y -= ny * correction;
                    }
                    else
                    {
                        double relativeVelocityX = velocity.X - other.velocity.X;
                        double relativeVelocityY = velocity.Y - other.velocity.Y;

                        double a = relativeVelocityX * relativeVelocityX + relativeVelocityY * relativeVelocityY;
                        double b = 2 * (dx * relativeVelocityX + dy * relativeVelocityY);
                        double c = distanceSquared - minDistance * minDistance;

                        // Пропускаем, если шары неподвижны относительно друг друга
                        if (a == 0) continue;

                        double discriminant = b * b - 4 * a * c;

                        // Если есть реальное столкновение в пределах одного кадра
                        if (discriminant >= 0)
                        {
                            double t = (-b - Math.Sqrt(discriminant)) / (2 * a);
                            if (t >= 0 && t <= 1)
                            {
                                // Перемещаем шары в точку столкновения
                                position.X -= velocity.X * t;
                                position.Y -= velocity.Y * t;
                                other.position.X -= other.velocity.X * t;
                                other.position.Y -= other.velocity.Y * t;

                                // Пересчитываем вектор между центрами в момент столкновения
                                dx = position.X - other.position.X;
                                dy = position.Y - other.position.Y;
                                distance = Math.Sqrt(dx * dx + dy * dy);

                                if (distance > 0)
                                {
                                    if (this is SpecialCircle && !(other is Paddle))
                                    {
                                        for (int i = 0; i < others.Length; i++)
                                        {
                                            if (others[i] == other)
                                            {
                                                stats.AddScore(100);
                                                for (int j = 0; j < others.Length; j++)
                                                {
                                                    if (others[j] == null)
                                                    {
                                                        others[j] = new PointObject(other.position.X, other.position.Y);
                                                        break;
                                                    }
                                                }
                                                others[i] = null;
                                                break;
                                            }
                                        }
                                        position.X += velocity.X * t;
                                        position.Y += velocity.Y * t;
                                    }
                                    else if (other is SpecialCircle && !(this is Paddle))
                                    {
                                        for (int i = 0; i < others.Length; i++)
                                        {
                                            if (others[i] == this)
                                            {
                                                stats.AddScore(100);
                                                for (int j = 0; j < others.Length; j++)
                                                {
                                                    if (others[j] == null)
                                                    {
                                                        others[j] = new PointObject(position.X, position.Y);
                                                        break;
                                                    }
                                                }
                                                others[i] = null;
                                                break;
                                            }
                                        }
                                    }
                                    double nx = dx / distance;
                                    double ny = dy / distance;

                                    // Пересчитываем относительную скорость
                                    relativeVelocityX = velocity.X - other.velocity.X;
                                    relativeVelocityY = velocity.Y - other.velocity.Y;
                                    double normalSpeed = relativeVelocityX * nx + relativeVelocityY * ny;

                                    if (normalSpeed < 0)
                                    {
                                        // Рассчет импульса
                                        double impulse = (2 * normalSpeed * mass * other.mass) / (mass + other.mass);

                                        // Обновление скоростей
                                        velocity.X -= (impulse / mass) * nx;
                                        velocity.Y -= (impulse / mass) * ny;
                                        other.velocity.X += (impulse / other.mass) * nx;
                                        other.velocity.Y += (impulse / other.mass) * ny;

                                        // Коррекция позиций
                                        double overlap = minDistance - distance;
                                        if (overlap > 0)
                                        {
                                            double correction = overlap / 2;
                                            position.X += nx * correction;
                                            position.Y += ny * correction;
                                            other.position.X -= nx * correction;
                                            other.position.Y -= ny * correction;
                                        }
                                    }

                                    // Возвращаем шары обратно в конец кадра
                                    position.X += velocity.X * t;
                                    position.Y += velocity.Y * t;
                                    other.position.X += other.velocity.X * t;
                                    other.position.Y += other.velocity.Y * t;
                                }
                            }
                        }
                    }
                }
            }
        }

        public double? TimeToCollision(DisplayObject other)
        {
            double dx = (other.Bounds.X + other.Size) - (Bounds.X + Size);
            double dy = (other.Bounds.Y + other.Size) - (Bounds.Y + Size);
            double dvx = other.VelocityX - VelocityX;
            double dvy = other.VelocityY - VelocityY;

            double a = dvx * dvx + dvy * dvy;
            if (a == 0) return null; // шары не движутся относительно друг друга

            double b = 2 * (dx * dvx + dy * dvy);
            double c = dx * dx + dy * dy - (Size + other.Size) * (Size + other.Size);

            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return null;

            double t = (-b - (double)Math.Sqrt(discriminant)) / (2 * a);
            return (t >= 0 && t <= 1.0d) ? t : (double?)null;
        }

        public void IncreaseSpeed(double factor)
        {
            velocity.X *= factor;
            velocity.Y *= factor;
        }

        public abstract void Draw(Graphics g);
    }

    public class Circle : DisplayObject
    {
        public Circle(double x, double y) : base(x, y) { }

        [JsonConstructor]
        public Circle(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        { }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(fillColor),
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
            g.DrawEllipse(borderPen,
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
        }
    }

    

    public class SpecialCircle : Circle
    {
        public SpecialCircle(double x, double y) : base(x, y)
        {
            size = 7;
            fillColor = Color.Black;
            borderThickness = 3;
            borderColor = Color.FromArgb(255, 0, 0);
            borderPen = new Pen(borderColor, (float)borderThickness); ;
            mass = 3;
            velocity.X = 0;
            velocity.Y = -3;
        }

        [JsonConstructor]
        public SpecialCircle(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        { }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(fillColor),
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
            g.DrawEllipse(borderPen,
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
        }
    }

    public class PointObject : DisplayObject
    {
        private string text;
        private Font font;
        private Brush textBrush;
        private Brush outlineBrush;

        public PointObject(double x, double y) : base(x, y)
        {
            text = "100";
            size = 15;
            font = new Font("Arial", 12, FontStyle.Bold);
            fillColor = Color.Yellow;
            textBrush = new SolidBrush(Color.Yellow);
            outlineBrush = new SolidBrush(Color.Black);
            borderThickness = 0;
            velocity = new PointD(0, 2);
            acceleration = new PointD(0, 0);
            mass = 0;
            isMoving = true;
            isAccelerating = false;
        }

        [JsonConstructor]
        public PointObject(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        {
            text = "100";
            font = new Font("Arial", 12, FontStyle.Bold);
            textBrush = new SolidBrush(Color.Yellow);
            outlineBrush = new SolidBrush(Color.Black);
        }

        [JsonProperty("Text")]
        public string Text => text;

        public override RectangleD Bounds
        {
            get
            {
                // Приблизительные границы для текста
                return new RectangleD(
                    position.X - size,
                    position.Y - size / 2,
                    size * 2,
                    size
                );
            }
        }

        public override void Draw(Graphics g)
        {
            // Рисуем обводку (смещением текста)
            float outlineOffset = 1f;
            g.DrawString(text, font, outlineBrush, (float)(position.X - size + outlineOffset), (float)(position.Y - size / 2 + outlineOffset));
            g.DrawString(text, font, outlineBrush, (float)(position.X - size - outlineOffset), (float)(position.Y - size / 2 - outlineOffset));
            g.DrawString(text, font, outlineBrush, (float)(position.X - size + outlineOffset), (float)(position.Y - size / 2 - outlineOffset));
            g.DrawString(text, font, outlineBrush, (float)(position.X - size - outlineOffset), (float)(position.Y - size / 2 + outlineOffset));
            // Рисуем основной текст
            g.DrawString(text, font, textBrush, (float)(position.X - size), (float)(position.Y - size / 2));
        }
    }

    public class Triangle : DisplayObject
    {
        public Triangle(double x, double y) : base(x, y) { }
        [JsonConstructor]
        public Triangle(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        { }
        public override void Draw(Graphics g)
        {
            PointF[] points = new PointF[]
            {
                new PointF((float)position.X, (float)(position.Y - size)),
                new PointF((float)(position.X - size), (float)(position.Y + size)),
                new PointF((float)(position.X + size), (float)(position.Y + size))
            };
            g.FillPolygon(new SolidBrush(fillColor), points);
            g.DrawPolygon(borderPen, points);
        }
    }

    public class Square : DisplayObject
    {
        public Square(double x, double y) : base(x, y) { }
        [JsonConstructor]
        public Square(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        { }
        public override void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(fillColor),
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
            g.DrawRectangle(borderPen,
                (float)(position.X - size), (float)(position.Y - size), (float)(size * 2), (float)(size * 2));
        }
    }

    public class Hexagon : DisplayObject
    {
        public Hexagon(double x, double y) : base(x, y) { }
        [JsonConstructor]
        public Hexagon(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        { }
        public override void Draw(Graphics g)
        {
            PointF[] points = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                points[i] = new PointF(
                    (float)(position.X + size * Math.Cos(i * Math.PI / 3)),
                    (float)(position.Y + size * Math.Sin(i * Math.PI / 3))
                );
            }
            g.FillPolygon(new SolidBrush(fillColor), points);
            g.DrawPolygon(borderPen, points);
        }
    }

    public class Paddle : DisplayObject
    {
        private double width;
        private bool isFrozen = false;
        private DateTime freezeEndTime;


        public Paddle(double x, double y) : base(x, y)
        {
            width = 75;
            size = 4; // Высота платформы (используется как половина высоты)
            fillColor = Color.Blue;
            borderThickness = 2;
            borderPen = new Pen(Color.Black, (float)borderThickness);
            isMoving = false; // Платформа не движется автоматически
            mass = 1000000; // Большая масса, чтобы платформа не двигалась при столкновении
        }

        [JsonConstructor]
        public Paddle(double positionX, double positionY, double velocityX, double velocityY,
        double accelerationX, double accelerationY, int colorR, int colorG, int colorB,
        int borderColorR, int borderColorG, int borderColorB,
        double borderThickness, double size, bool isAccelerating, bool isMoving, double mass, double width)
        : base(positionX, positionY, velocityX, velocityY, accelerationX, accelerationY,
               colorR, colorG, colorB, borderColorR, borderColorG, borderColorB,
               borderThickness, size, isAccelerating, isMoving, mass)
        {
            this.width = width;
        }

        [JsonProperty("Width")]
        public double Width => width;

        public override RectangleD Bounds
        {
            get
            {
                return new RectangleD(
                    position.X - width / 2,
                    position.Y - size,
                    width,
                    size * 2
                );
            }
        }

        public void UpdatePosition(int newX, Rectangle bounds)
        {
            if (!isFrozen)
            {
                position.X = Math.Max(bounds.Left + width / 2, Math.Min(bounds.Right - width / 2, newX));
            }
            velocity.X = 0;
            velocity.Y = 0;

            // Проверяем, истекло ли время заморозки
            if (isFrozen && DateTime.Now >= freezeEndTime)
            {
                isFrozen = false;
            }
        }

        public void UpdateY(double newY)
        {
            position.Y = newY;
        }
        public void IncreaseSize(double factor)
        {
            width *= factor;
            // Ограничиваем минимальный и максимальный размер
            width = Math.Max(30, Math.Min(150, width));
        }

        public void Freeze(int durationMs)
        {
            isFrozen = true;
            freezeEndTime = DateTime.Now.AddMilliseconds(durationMs);
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(new SolidBrush(fillColor),
                (float)(position.X - width / 2), (float)(position.Y - size), (float)width, (float)(size * 2));
            g.DrawRectangle(borderPen,
                (float)(position.X - width / 2), (float)(position.Y - size), (float)width, (float)(size * 2));
        }
    }
}