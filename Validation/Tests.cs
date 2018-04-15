using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Araye.CommonTests.Validation
{
    /// <summary>
    /// collection of general test methods for data annotations attributes
    /// </summary>
    public class Tests
    {
        /// <summary>
        /// check if model is valid
        /// </summary>
        /// <param name="model">model to validate</param>
        /// <returns>list of validation errors</returns>
        public static IList<ValidationResult> Validate(object model)
        {
            var results = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, results, true);
            if (model is IValidatableObject) (model as IValidatableObject).Validate(validationContext);
            return results;
        }

        [TestMethod]
        public void Validate_Model_Given_ExpectNoValidationErrors(object model)
        {
            var results = Validate(model);
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void Validate_Model_Given_ExpectSomeValidationErrors(object model,int expectedNumberOfErrors)
        {
            var results = Validate(model);
            Assert.AreEqual(expectedNumberOfErrors, results.Count);
        }

        [TestMethod]
        public void Validate_Model_Given_ExpectHavingSpecificValidationError(object model, string errorMessage)
        {
            var results = Validate(model);
            Assert.IsTrue(results.Any(r=>r.ErrorMessage.ToLower()==errorMessage.ToLower()));
        }
    }
}
