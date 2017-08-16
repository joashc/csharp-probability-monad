using static ProbCSharp.ProbBase;

namespace ProbCSharp.Test.Models
{
    public class Grade
    {
        public double GPA;
        public Country Country;
        public Grade(double gpa, Country country)
        {
            GPA = gpa;
            Country = country;
        }
    }

    public enum Country { USA, India };

    public static class IndianGpaModel
    {
        public static Dist<Grade>
        UsaGpa =
            from atMinMax in Bernoulli(0.05)
            from gpa in atMinMax ? Bernoulli(0.85, 1.0, 0.0): Beta(2, 8)
            select new Grade(gpa * 4, Country.USA);

        public static Dist<Grade>
        IndiaGpa =
            from atMinMax in Bernoulli(0.01)
            from gpa in atMinMax ? Bernoulli(0.9, 1.0, 0.0): Beta(5, 5)
            select new Grade(gpa * 10, Country.India);
    }
}
