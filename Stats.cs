using System;
using System.Drawing;
using Newtonsoft.Json;

namespace ootpisp { 
    public class Stats { 
        private int totalScore; 
        private Font font; 
        private Brush textBrush; 
        private Brush outlineBrush; 

    public Stats()
    {
        totalScore = 0;
        font = new Font("Arial", 14, FontStyle.Bold);
        textBrush = new SolidBrush(Color.Yellow);
        outlineBrush = new SolidBrush(Color.Black);
    }

    [JsonConstructor]
    public Stats(int totalScore)
    {
        this.totalScore = totalScore;
        font = new Font("Arial", 14, FontStyle.Bold);
        textBrush = new SolidBrush(Color.Yellow);
        outlineBrush = new SolidBrush(Color.Black);
    }

    [JsonProperty("TotalScore")]
    public int TotalScore => totalScore;

    public void AddScore(int points)
    {

        totalScore += (int)(points);
    }

    public void Reset()
    {
        totalScore = 0;
    }

    public void Draw(Graphics g, Rectangle bounds)
    {
        string scoreText = $"Очки: {totalScore}";
        float x = bounds.Right - 100;
        float y = bounds.Top + 10;

        float outlineOffset = 1f;
        g.DrawString(scoreText, font, outlineBrush, x + outlineOffset, y + outlineOffset);
        g.DrawString(scoreText, font, outlineBrush, x - outlineOffset, y - outlineOffset);
        g.DrawString(scoreText, font, outlineBrush, x + outlineOffset, y - outlineOffset);
        g.DrawString(scoreText, font, outlineBrush, x - outlineOffset, y + outlineOffset);
        g.DrawString(scoreText, font, textBrush, x, y);
    }
}

}