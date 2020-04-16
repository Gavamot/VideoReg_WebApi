using System;

namespace WebApi.Archive.BrigadeHistory
{
    public interface IBrigadeHistory
    {
        int GetBrigadeCode(DateTime pdt);
        bool IsEmptyCode(int brigade);
    }
}
