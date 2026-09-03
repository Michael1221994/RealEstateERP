using System.Reflection;

namespace SES.WINSSAS.Common.Enums
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DocumentTypeCode : Attribute
    {
        public string Code { get; }
        public DocumentTypeCode(string code)
        {
            Code = code;
        }
    }

    public enum DocumentTypeEnum
    {
        // --- Organization & Corporate Structure ---
        [DocumentTypeCode("ORGPROC")] EstablishmentProclamation,
        [DocumentTypeCode("ORGAPPL")] OrganizationApplicationLetter,
        [DocumentTypeCode("ORGBUSL")] BusinessLicense,
        [DocumentTypeCode("ORGTINC")] TinCertificate,
        [DocumentTypeCode("ORGARTA")] ArticleOfAssociation,
        [DocumentTypeCode("ORGBREG")] BusinessRegistrationCertificate,
        [DocumentTypeCode("SPLMCRT")] SplitMergerCertificate,
        [DocumentTypeCode("SPLMSUP")] SplitMergerSupportingDoc,

        // --- Employee Base Documents ---
        [DocumentTypeCode("EMPLTTR")] EmploymentLetter,
        [DocumentTypeCode("EMPHIST")] PersonalHistory,
        [DocumentTypeCode("EMPPHOT")] EmployeePhoto,
        [DocumentTypeCode("EMPFAYD")] FaydaPhoto,
        [DocumentTypeCode("EMPDETH")] DeathCertificate,
        [DocumentTypeCode("EMPEXTN")] ExtensionFile, // For service extension
        [DocumentTypeCode("EMPSALC")] SalaryChangeFile, // From ContributionDeclarationDetail (Row level)
        [DocumentTypeCode("SALCHGL")] SalaryChangeLetter, // From SalaryHistory (Request level)
        [DocumentTypeCode("OLDEMPL")] OldEmploymentLetter, // Transfer Request
        [DocumentTypeCode("NEWEMPL")] NewEmploymentLetter, // Transfer Request
        [DocumentTypeCode("EMPLNMC")] NoNMarriedCertificate,
        [DocumentTypeCode("EMPLCRT")] EmployeeCertificate,
        [DocumentTypeCode("EMPFRM1")] Form1,
        [DocumentTypeCode("ENTFRM2")] Form2,




        // --- Service & Termination ---
        [DocumentTypeCode("TRMFILE")] TerminationFile,
        [DocumentTypeCode("SRVLTR")] ServiceLetter,
        [DocumentTypeCode("CLRNC")] ServiceClearance, // From EmploymentHistory
        [DocumentTypeCode("CLRNCLTR")] ContributionClearanceLetter, // From ContributionDeclarationClearance

        // --- Family Members (Spouse, Child, Parent) ---
        [DocumentTypeCode("FAMMARR")] MarriageCertificate,
        [DocumentTypeCode("FAMDIVR")] DivorceCertificate,
        [DocumentTypeCode("SPPHOT")] SpousePhoto,
        [DocumentTypeCode("CHPHOT")] ChildPhoto,
        [DocumentTypeCode("FAMADPT")] AdoptionDocument,
        [DocumentTypeCode("FAMDISB")] DisabilityDocument,
        [DocumentTypeCode("FAMBRTH")] ChildbirthCertificate,
        [DocumentTypeCode("PARVERI")] ParentVerificationFile,

        // --- Contributions & Financials ---
        [DocumentTypeCode("CONDECL")] ContributionDeclarationFile,
        [DocumentTypeCode("PAYPRFF")] PaymentProofFile, // From PaidDeclaration
        [DocumentTypeCode("BNKORDR")] BankOrderFile,
        [DocumentTypeCode("PENALTY")] PenaltyLetter,
        [DocumentTypeCode("PAYROLL")] BeneficiaryPayrollFile, // From BeneficiaryPayrollBatch
        [DocumentTypeCode("VCHRFIL")] PaymentVoucherFile, // Generated document for a PaymentVoucher
        [DocumentTypeCode("FFMS_Supporting_Document")] FFMSSupportingDocument, // Supporting doc sent to/from FFMS

        // --- Entitlement & Claims ---
        [DocumentTypeCode("ENTDOC")] EntitlementDocument,
        [DocumentTypeCode("ENTINSR")] InsuranceDocument,
        [DocumentTypeCode("ENTRSGF")] SelfResignationForm,
        [DocumentTypeCode("ENTRSGL")] CompanyResignationLetter,
        [DocumentTypeCode("ENTMEDG")] MedicalDamageReport,
        [DocumentTypeCode("ENTDAMN")] DamageNotificationForm,
        [DocumentTypeCode("ENTPOLC")] PoliceReport,
        [DocumentTypeCode("ENTVALID")] ValidId,
        [DocumentTypeCode("ENTMEDC")] MedicalCertificate,
        [DocumentTypeCode("ENTAPPL")] ApplicationForm,
        [DocumentTypeCode("ENTBANK")] BankAccountDetails,
        [DocumentTypeCode("ENTSHEET")] EntitlementSheet, // From Beneficiary
        [DocumentTypeCode("ADJSUPP")] AdjustmentSupportingDoc, // From EntitlementRequestAdjustmentDocument

        // --- Delegation & Address Change ---
        [DocumentTypeCode("TRNSLTR")] TransferLetter, // From AddressChangeRequest
        [DocumentTypeCode("DELEGDOC")] DelegationEvidence, // From DelegationDocument/Request
        [DocumentTypeCode("BENEDOC")] BeneficiaryDocument,
        [DocumentTypeCode("PROXDOC")] ProxyIdentification,

        // --- Miscellaneous ---
        [DocumentTypeCode("NOTIFAT")] NotificationAttachment,
        [DocumentTypeCode("GENERIC")] GenericDocument,

        // --- Contributions & Refunds ---
        [DocumentTypeCode("REFAPPL")] RefundApplicationForm,
        [DocumentTypeCode("REFSUPP")] RefundSupportingDoc,
        [DocumentTypeCode("ORGBNKC")] OrganizationBankConfirmation,

        // --- Identity & Legal ---
        [DocumentTypeCode("IDSCAN")] NationalIdScan,
        [DocumentTypeCode("PASSPORT")] PassportScan,
        [DocumentTypeCode("POADOC")] PowerOfAttorney,
        [DocumentTypeCode("SURVPRF")] SurvivalProofDocument,

        [DocumentTypeCode("PROCDOC")] ProclamationDocument,


        // Investment Documents
        [DocumentTypeCode("IVSTMP")] InvestmentPolicy,
        [DocumentTypeCode("IVSTMMA")] InvestmentMarketAnalysis, // saved analysis
        [DocumentTypeCode("MTNCRD")] MaintenanceRequestDocument,
        [DocumentTypeCode("TNTAGR")] TenantAgreementDocument,

        // NOTE: values are persisted as int (HasConversion<int>) — only append new
        // members below this line, never insert above it.
        [DocumentTypeCode("BENBNKIJ")] BeneficiaryBankInactivityJustification, // Beneficiary justification for bank inactivity
        [DocumentTypeCode("TNTCLR")] TenantClearanceDocument,
        [DocumentTypeCode("PPSUPP")] PurchasePlanSupportingDocument, // Supporting doc attached to a purchase plan
        [DocumentTypeCode("APPLSUPP")] AppealSupportingDocument, // Evidence a caseworker attaches when handing an appeal on
        [DocumentTypeCode("ENTNDTL")] NonDisciplinaryTerminationLetter, // Employer letter certifying the service did not end in a disciplinary termination

        // --- Payment request supporting documents ---
        // Two kinds rather than one, because finance reads them differently on review: an invoice
        // evidences the amount, a contract the obligation.
        [DocumentTypeCode("PRINV")] PaymentRequestInvoice,
        [DocumentTypeCode("PRCONT")] PaymentRequestContract,
    }

    public static class EnumHelper
    {
        public static string GetDocumentTypeCode<T>(T value) where T : Enum
        {
            var fieldInfo = typeof(T).GetField(value.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<DocumentTypeCode>();

            return attribute != null ? attribute.Code : value.ToString();
        }
    }
}