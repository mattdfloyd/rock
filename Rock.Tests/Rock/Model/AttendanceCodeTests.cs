using Rock.Model;
using Xunit;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode that does not need a database context.
    /// </summary>
    public class AttendanceCodeTests
    {
        #region Tests that don't require a database/context

        /// <summary>
        /// Avoids the triple six.  Note: Does not use the database or RockContext.
        /// </summary>
        [Fact]
        public void AvoidTripleSix()
        {
            int alphaNumericLength = 0;
            int alphaLength = 0;
            int numericLength = 4;
            bool isRandomized = false;
            string lastCode = "0665";

            string code = AttendanceCodeService.GetNextNumericCodeAsString( alphaNumericLength, alphaLength, numericLength, isRandomized, lastCode );
            Assert.Equal( "0667", code );
        }

        #endregion

    }
}
