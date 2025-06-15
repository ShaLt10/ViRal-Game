public class SpotTheDifferenceStatusData
{
    public bool gameStart;
    public bool gameWin;
    public bool gameFinished;

    public SpotTheDifferenceStatusData(bool gameStart, bool gameWin, bool gamefinished)
    {
        this.gameStart = gameStart;
        this.gameWin = gameWin;
        this.gameFinished = gameWin;
    }
}
