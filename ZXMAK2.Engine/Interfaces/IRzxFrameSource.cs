using ZXMAK2.Entities;


namespace ZXMAK2.Engine.Interfaces
{
    public interface IRzxFrameSource
    {
        RzxFrame[] GetNextFrameArray();
    }
}
