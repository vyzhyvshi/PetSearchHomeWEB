//Тест для аналізатора
namespace PetSearchHome_WEB.Models.Auth
{
    public class LoginViewModel
    {
        public bool IsOk(bool flag)
        {
            if (flag == true)
                return true;
            else
                return false;
        }
    }
}
