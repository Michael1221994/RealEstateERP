using Infrastracture.Base.BaseEntities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.BaseEntities
{
    public class PersonalProfileBase
    {
        public string ID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public SexEnum? Sex { get; set; }
        public Guid UUID { get { return !string.IsNullOrEmpty(ID)? new Guid(this.ID):Guid.Empty; } }
        public Guid? CourtID { get; set; }
        public Guid? BenchID { get; set; }

    }
    public class PersonalInformationBase: PersonalProfileBase
    {
        public string? Title { get; set; }
        // Identification Details
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Ethnicity { get; set; }
        public string? MaritalStatus { get; set; } // Single, Married, Divorced, etc.

        // Contact Information
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternativePhoneNumber { get; set; }
        // Address Information (Nested Class)
        public Address HomeAddress { get; set; }
        public Address? WorkAddress { get; set; } // Optional Work Address

        // Employment & Education
        public string? Occupation { get; set; }
        public string? Employer { get; set; }
        public string? JobTitle { get; set; }
        public string? EducationLevel { get; set; } // High School, Bachelor's, Master's, etc.

        // Health & Legal Information
        public string? BloodType { get; set; }
        public bool? HasDisabilities { get; set; }
        public string? MedicalConditions { get; set; } // Optional medical details
        public bool? IsLegalResident { get; set; }
        public string? PassportNumber { get; set; }
        public string? DriverLicenseNumber { get; set; }

        // Optional Metadata
        public DateTime? DateOfRegistration { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Constructor
        public PersonalInformationBase()
        {
            HomeAddress = new Address();
            WorkAddress = null; // Optional
        }
    }

    // Detailed Address Class
    public class Address
    {
        public string? Country { get; set; } = string.Empty;
        public string? StateOrProvince { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Subcity { get; set; } = string.Empty;
        public string? Woreda { get; set; } = string.Empty;
        public string? Kebele { get; set; }
        public string? Street { get; set; } = string.Empty;
        public string? PostalCode { get; set; }

        // Optional Additional Address Details
        public string? HouseNumber { get; set; }
        public string? Landmark { get; set; } // Nearby recognizable place
    }
}
