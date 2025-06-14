
using LS.FileReader.Attributes;

namespace LS.FileReader.Tests.Dtos
{
    public class TestPerson
    {
        [HeaderColumn("Name", "FullName")]
        public string Name { get; set; }

        [HeaderColumn("Age")]
        public int Age { get; set; }

        [HeaderColumn("DOB")]
        public DateTime BirthDate { get; set; }
    }
}
