using Swashbuckle.Swagger.Annotations;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace calcAPI2017.Controllers
{
    /// <summary>
    /// A simple calculator api by apollak
    /// </summary>
    public class CalcController : ApiController
    {
        /// <summary>
        /// This call allows to add two integers together
        /// </summary>
        /// <param name="a">First integer to add</param>
        /// <param name="b">Second integer to add</param>
        /// <returns>The result of a+b</returns>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult Add(int a, int b)
        {
            return Ok(a + b);
        }

        /// <summary>
        /// This substracts values from each other!
        /// </summary>
        /// <param name="a">Value b is substracted from this value.</param>
        /// <param name="b">Value which is substracted from a.</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(int))]
        public IHttpActionResult Subtract(int a, int b)
        {
            return Ok(a - b);
        }

        /// <summary>
        /// Multiply the values a and b.
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns></returns>
        [HttpGet]
        public int Multiply(int a, int b)
        {
            return a * b;
        }

        /// <summary>
        /// Divides the values a / b
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.BadRequest, "b cannot be 0 because you cannot divide by 0!")]
        public int Divide(int a, int b)
        {
            return a / b;
        }
    }
}
