namespace CharityPay.Domain.Enums;

public enum DocumentType
{
    CorporateDocument = 0,  // KRS, statut, umowa
    GovernmentId = 1,       // dowód osobisty, paszport
    BankStatement = 2,      // wyciąg bankowy
    TaxCertificate = 3,     // zaświadczenie z urzędu skarbowego
    AuthorizationLetter = 4 // pełnomocnictwo
}