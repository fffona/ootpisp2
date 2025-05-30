using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ootpisp
{
    public class Stage
    {
        private DisplayObject[] gameObjects;
        private Rectangle bounds;
        private Brush borderBrush = Brushes.DimGray;
        private Brush backgroundBrush;
        private Image backgroundImage;
        private enum BackgroundType { SolidColor, Gradient, Image }
        private BackgroundType currentBackgroundType;
        private Stats stats;
        public Stage(int width, int height)
        {
            bounds = new Rectangle(30, 30, width - 60, height - 60);
            gameObjects = new DisplayObject[350];
            backgroundBrush = Brushes.White;
            stats = new Stats();
            currentBackgroundType = BackgroundType.SolidColor;
        }
        
        [JsonConstructor]
        public Stage(Rectangle bounds, DisplayObject[] gameObjects, Stats stats)
        {
            this.bounds = bounds;
            this.gameObjects = gameObjects ?? new DisplayObject[0];
            this.stats = stats ?? new Stats();
            backgroundBrush = Brushes.White;
            currentBackgroundType = BackgroundType.SolidColor;
        }

        [JsonProperty("Stats")]
        public Stats Stats => stats;

        public void Resize(int width, int height)
        {
            bounds = new Rectangle(30, 30, width - 60, height - 60);
            foreach (var obj in gameObjects)
            {
                if (obj is Paddle paddle)
                {
                    paddle.UpdateY(bounds.Height - 20);
                }
            }
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundBrush = new SolidBrush(color);
            backgroundImage = null;
            currentBackgroundType = BackgroundType.SolidColor;
        }

        public void SetBackgroundGradient(Color color1, Color color2)
        {
            backgroundBrush = new LinearGradientBrush(bounds, color1, color2, LinearGradientMode.Vertical);
            backgroundImage = null;
            currentBackgroundType = BackgroundType.Gradient;
        }

        public void SetBackgroundImage(string imagePath)
        {
            if (File.Exists(imagePath))
            {
                backgroundImage = Image.FromFile(imagePath);
                backgroundBrush = null;
                currentBackgroundType = BackgroundType.Image;
            }
        }

        public void ChangeAmount(int amount)
        {
            gameObjects = new DisplayObject[2 * amount + 2];
        }


        public void AddObject(DisplayObject obj)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == null)
                {
                    gameObjects[i] = obj;
                    break;
                }
            }
        }

        public void RemoveObject(DisplayObject obj)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] == obj)
                {
                    gameObjects[i] = null;
                    break;
                }
            }
        }

        public void ClearObjects()
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i] = null;
            }
            stats.Reset();
        }

        public DisplayObject[] GetObjects()
        {
            return gameObjects;
        }

        public void Update()
        {
            foreach (var obj in gameObjects)
            {
                if (obj != null) obj.Move(bounds, gameObjects, stats);
            }
        }

        public void UpdatePaddlePosition(int mouseX)
        {
            foreach (var obj in gameObjects)
            {
                if (obj is Paddle paddle)
                {
                    paddle.UpdatePosition(mouseX, bounds);
                    break;
                }
            }
        }

        public void ToggleAccelerationForAll()
        {
            foreach (var obj in gameObjects)
            {
                if (!(obj is Paddle))
                {
                    obj?.ToggleAcceleration();
                }
            }
        }

        public void DisableAccelerationForAll()
        {
            foreach (var obj in gameObjects)
            {
                if (!(obj is Paddle))
                {
                    obj?.DisableAcceleration();
                }
            }
        }

        public void Draw(Graphics g)
        {
            // Рисуем черную рамку
            g.FillRectangle(borderBrush, -3, -3, bounds.Width + 63, bounds.Height + 63);
            if (currentBackgroundType == BackgroundType.SolidColor || currentBackgroundType == BackgroundType.Gradient)
            {
                g.FillRectangle(backgroundBrush, bounds);
            }
            else if (currentBackgroundType == BackgroundType.Image && backgroundImage != null)
            {
                g.DrawImage(backgroundImage, bounds);
            }

            // Рисуем все объекты
            foreach (var obj in gameObjects)
            {
                if (obj != null)
                {
                    obj.Draw(g);
                }
            }

            stats.Draw(g, bounds);
        }
    }
}