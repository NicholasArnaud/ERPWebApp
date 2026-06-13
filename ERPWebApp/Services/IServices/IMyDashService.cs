using ERPWebApp.Data.DTOModels;
using Microsoft.AspNetCore.Mvc;

namespace ERPWebApp.Services.IServices
{
    public interface IMyDashService
    {
        public Task<MyDashDTO> GetUserDashboadData(string userId);
        public Task<MyDashDTO> SaveUserCustomDashBoard(MyDashDTO myDashDTO, string userId);
        public Task<IActionResult> UpdateFavouriteStatus(string propertyName, bool value, string userId);
    }
}
