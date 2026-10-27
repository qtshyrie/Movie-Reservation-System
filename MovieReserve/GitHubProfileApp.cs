using System;
using System.Drawing;
using System.Windows.Forms;

public class GitHubProfilePanel : Panel
{
    // Properties to hold dynamic info for any person
    public string Username { get; set; } = "cristopher@ramirez";
    public string Title { get; set; } = "student";
    public string Major { get; set; } = "Computer Science";
    public string Education { get; set; } = "University of Caloocan City";
    public string Focuss { get; set; } = "Web and Software Development";
    public string Experience { get; set; } = "2 yrs student";

    public string ProgrammingLangs { get; set; } = "Java, C, C#";
    public string RealLangs { get; set; } = "Filipino, English";

    public string InterestTech { get; set; } = "Software Dev, Electronics";
    public string InterestNonTech { get; set; } = "Eating, Egames, Any Sports";

    public string FacebookUser { get; set; } = "Ramirez Cristopher";
    public string EmailPersonal { get; set; } = "cristopherram55@gmail.com";
    public string InstagramUser { get; set; } = "@itzm_yvon";
    public string GithubUser { get; set; } = "qtshyrie";

    // Keep or adjust these stats as needed
    public string RepoCount { get; set; } = "15 {Contributed: 25}";
    public string StarsCount { get; set; } = "42";
    public string CommitCount { get; set; } = "512";
    public string FollowersCount { get; set; } = "28";
    public string TotalLoc { get; set; } = "45,000";
    public string AddedLoc { get; set; } = "38,000++";
    public string DeletedLoc { get; set; } = "7,000--";

    public GitHubProfilePanel()
    {
        this.Width = 1067;
        this.Height = 745;
        this.BackColor = Color.FromArgb(13, 17, 23);
        this.DoubleBuffered = true;
    }
    // Optional ASCII art (empty by default)
    public string[] CustomAsciiArt { get; set; } = new string[0];

    // Optional profile photo, drawn on the left with an orange frame
    public Image Photo { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using (Font font = new Font("Consolas", 10F, FontStyle.Regular))
        using (Font boldFont = new Font("Consolas", 10F, FontStyle.Bold))
        {
            Brush artBrush = new SolidBrush(Color.FromArgb(88, 166, 255));
            Brush headerBrush = new SolidBrush(Color.FromArgb(240, 246, 252));
            Brush keyBrush = new SolidBrush(Color.FromArgb(210, 153, 34));
            Brush valueBrush = new SolidBrush(Color.FromArgb(201, 209, 217));
            Brush greenBrush = new SolidBrush(Color.FromArgb(63, 185, 80));
            Brush redBrush = new SolidBrush(Color.FromArgb(248, 81, 73));

            int startX = 25;
            int startY = 25;
            int lineHeight = 21;

            // Use the developer's unique ASCII art property here!
            string[] asciiArt = CustomAsciiArt;

            string[] profileLines = new string[]
            {
                $"{Username} ----------------------------------------------------",
                $". Title: ....................... {Title}",
                $". Major: ....................... {Major}",
                $". Education: ................... {Education}",
                $". Focus: ....................... {Focuss}",
                $". Experience: .................. {Experience}",
                $". Languages.Programming: ..... {ProgrammingLangs}",
                $". Languages.Real: .......................... {RealLangs}",
                $". Interest.Tech: ............... {InterestTech}",
                $". Interest.Non-Tech: ........... {InterestNonTech}",
                "",
                "- Contact & Socials --------------------------------------------",
                $". Facebook: .................... {FacebookUser}",
                $". Email.Personal: .............. {EmailPersonal}",
                $". Instagram: ................... {InstagramUser}",
                $". GitHub: ...................... {GithubUser}",
                "",
                "- GitHub Stats -------------------------------------------------",
                $". Repos: ..................... {RepoCount} | Stars: ........... {StarsCount}",
                $". Commits: ................... {CommitCount} | Followers: ....... {FollowersCount}",
                $". Lines of Code on GitHub:. {TotalLoc} ( {AddedLoc},  {DeletedLoc} )"
            };

            // Draw photo (cropped to fill the frame)
            if (Photo != null)
            {
                var frame = new Rectangle(startX, startY, 315, 425);
                float scale = Math.Max(frame.Width / (float)Photo.Width, frame.Height / (float)Photo.Height);
                float sw = frame.Width / scale, sh = frame.Height / scale;
                var src = new RectangleF((Photo.Width - sw) / 2f, (Photo.Height - sh) / 2f, sw, sh);
                g.DrawImage(Photo, frame, src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel);
                using (var pen = new Pen(Color.FromArgb(247, 148, 84), 3))
                    g.DrawRectangle(pen, frame);
            }

            // Draw ASCII Art
            for (int i = 0; i < asciiArt.Length; i++)
            {
                g.DrawString(asciiArt[i], font, artBrush, startX, startY + (i * lineHeight));
            }

            // Draw Profile Info
            int rightX = startX + 410;
            for (int i = 0; i < profileLines.Length; i++)
            {
                string line = profileLines[i];
                int currentY = startY + (i * lineHeight);

                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.Contains("----------------------------------------------------") || line.StartsWith("- Contact") || line.StartsWith("- GitHub Stats"))
                {
                    g.DrawString(line, boldFont, headerBrush, rightX, currentY);
                }
                else
                {
                    int colonIdx = line.IndexOf(':');
                    if (colonIdx != -1)
                    {
                        string keyPart = line.Substring(0, colonIdx + 1);
                        string valPart = line.Substring(colonIdx + 1);

                        g.DrawString(keyPart, font, keyBrush, rightX, currentY);
                        int keyWidth = TextRenderer.MeasureText(keyPart, font).Width - 8;
                        g.DrawString(valPart, font, valueBrush, rightX + keyWidth, currentY);
                    }
                    else
                    {
                        g.DrawString(line, font, valueBrush, rightX, currentY);
                    }
                }
            }
        }
    }
}