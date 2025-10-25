namespace mvcproducts.Controllers
{
    public class Class
    {

        public int sumar(int a, int b, int operacion) {
            if (operacion == 1)
            {
                return a + b;
            }
            else if (operacion == 2)
            {
                return a - b;
            }
            else if (operacion == 3)
            {
                return a * b;
            }
            else {
                return 0;
            }
        }
    }
}
