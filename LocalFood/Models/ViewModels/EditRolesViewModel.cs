using Microsoft.AspNetCore.Mvc.Rendering;

namespace LocalFood.ViewModels
{
    public class EditRolesViewModel
    {
        public string UserId  { get; set; }
        public string Email   { get; set; }
        public string Roles   { get; set; }            // вибрана роль
        public List<SelectListItem> AllRoles { get; set; }
    }
}