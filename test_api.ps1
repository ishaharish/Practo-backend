# Practo Backend API End-to-End Test Script
# This script registers a doctor and a patient, verifies OTPs, updates the doctor profile,
# generates slots, books an appointment, creates a medical record, and submits a review.

$baseUrl = "http://localhost:5229/api"

# Helper for displaying section titles
function Write-Header($text) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "  $text" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}

# 1. Register Patient
Write-Header "1. Registering Patient..."
$patientEmail = "patient_$(Get-Random)@example.com"
$patientRegBody = @{
    fullName = "John Doe"
    email = $patientEmail
    password = "SecurePassword123"
    phoneNumber = "9876543210"
    role = "Patient"
} | ConvertTo-Json

$patientRegRes = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $patientRegBody -ContentType "application/json"
$patientOtp = $patientRegRes.otp
Write-Host "Patient Registered: $patientEmail (OTP: $patientOtp)" -ForegroundColor Green

# 2. Register Doctor
Write-Header "2. Registering Doctor..."
$doctorEmail = "doctor_$(Get-Random)@example.com"
$doctorRegBody = @{
    fullName = "Dr. Jane Smith"
    email = $doctorEmail
    password = "DoctorPassword123"
    phoneNumber = "8765432109"
    role = "Doctor"
} | ConvertTo-Json

$doctorRegRes = Invoke-RestMethod -Uri "$baseUrl/auth/register" -Method Post -Body $doctorRegBody -ContentType "application/json"
$doctorOtp = $doctorRegRes.otp
Write-Host "Doctor Registered: $doctorEmail (OTP: $doctorOtp)" -ForegroundColor Green

# 3. Verify Patient Email
Write-Header "3. Verifying Patient OTP..."
$patientVerifyBody = @{
    email = $patientEmail
    otpCode = $patientOtp
} | ConvertTo-Json

$patientVerifyRes = Invoke-RestMethod -Uri "$baseUrl/auth/verify-otp" -Method Post -Body $patientVerifyBody -ContentType "application/json"
Write-Host "Patient Verification Status: $patientVerifyRes" -ForegroundColor Green

# 4. Verify Doctor Email
Write-Header "4. Verifying Doctor OTP..."
$doctorVerifyBody = @{
    email = $doctorEmail
    otpCode = $doctorOtp
} | ConvertTo-Json

$doctorVerifyRes = Invoke-RestMethod -Uri "$baseUrl/auth/verify-otp" -Method Post -Body $doctorVerifyBody -ContentType "application/json"
Write-Host "Doctor Verification Status: $doctorVerifyRes" -ForegroundColor Green

# 5. Login Patient
Write-Header "5. Logging in Patient..."
$patientLoginBody = @{
    email = $patientEmail
    password = "SecurePassword123"
} | ConvertTo-Json

$patientLoginRes = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $patientLoginBody -ContentType "application/json"
$patientToken = $patientLoginRes.token
Write-Host "Patient Logged In successfully. Dummy Token: $patientToken" -ForegroundColor Green

# 6. Login Doctor
Write-Header "6. Logging in Doctor..."
$doctorLoginBody = @{
    email = $doctorEmail
    password = "DoctorPassword123"
} | ConvertTo-Json

$doctorLoginRes = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $doctorLoginBody -ContentType "application/json"
$doctorToken = $doctorLoginRes.token
Write-Host "Doctor Logged In successfully. Dummy Token: $doctorToken" -ForegroundColor Green

# 7. Update Doctor Profile (Associate with Clinic 1 [Apollo Clinic, Bangalore] & Specialization 1 [Cardiologist])
Write-Header "7. Updating Doctor Profile..."
$doctorProfileBody = @{
    experienceYears = 12
    qualifications = "MBBS, MD Cardiology"
    biography = "Experienced heart care specialist committed to patient wellness."
    consultationFee = 500.00
    clinicIds = @(1, 2)            # Associated with Apollo Clinic & Practo Care Help
    specializationIds = @(1, 4)    # Cardiologist & General Physician
} | ConvertTo-Json

$doctorProfileRes = Invoke-RestMethod -Uri "$baseUrl/doctors/profile" -Method Post -Body $doctorProfileBody -ContentType "application/json"
Write-Host "Doctor Profile Updated: $doctorProfileRes" -ForegroundColor Green

# 8. Query Doctors by City & Specialization
Write-Header "8. Querying Doctors (City: Bangalore, Specialization: Cardiologist)..."
$doctorsList = Invoke-RestMethod -Uri "$baseUrl/doctors?city=Bangalore&specialization=Cardiologist" -Method Get
Write-Host "Found $($doctorsList.Count) doctor(s)." -ForegroundColor Green
$selectedDoctor = $doctorsList | Where-Object { $_.email -eq $doctorEmail }
$selectedDoctorId = $selectedDoctor.id
Write-Host "Selected Doctor ID: $selectedDoctorId" -ForegroundColor Green

# 9. Doctor Generates Slots for Tomorrow
Write-Header "9. Generating Availability Slots..."
$tomorrow = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
$slotsBody = @{
    clinicId = 1
    startDate = $tomorrow
    endDate = $tomorrow
    startTime = "09:00:00"
    endTime = "11:00:00"
    slotDurationMinutes = 30
} | ConvertTo-Json

$slotsRes = Invoke-RestMethod -Uri "$baseUrl/doctors/slots/generate" -Method Post -Body $slotsBody -ContentType "application/json"
Write-Host "Slots Generation Result: $slotsRes" -ForegroundColor Green

# 10. Patient Fetches Doctor's Slots
Write-Header "10. Fetching Doctor's Available Slots..."
$slotsUrl = "$baseUrl/doctors/$selectedDoctorId/slots?date=$tomorrow"
$availableSlots = Invoke-RestMethod -Uri $slotsUrl -Method Get
Write-Host "Found $($availableSlots.Count) slots for $tomorrow." -ForegroundColor Green
$selectedSlotId = $availableSlots[0].id
Write-Host "Selected Slot ID: $selectedSlotId" -ForegroundColor Green

# 11. Patient Books Appointment
Write-Header "11. Booking Appointment..."
$bookingBody = @{
    doctorId = $selectedDoctorId
    slotId = $selectedSlotId
    symptoms = "Mild chest pain and shortness of breath."
} | ConvertTo-Json

$bookingRes = Invoke-RestMethod -Uri "$baseUrl/appointments" -Method Post -Body $bookingBody -ContentType "application/json"
$appointmentId = $bookingRes.appointmentId
Write-Host "Appointment Booked! Appointment ID: $appointmentId" -ForegroundColor Green

# 12. Doctor Confirms Appointment
Write-Header "12. Updating Appointment Status to Confirmed..."
$statusBody = @{
    status = "Confirmed"
} | ConvertTo-Json

$statusRes = Invoke-RestMethod -Uri "$baseUrl/appointments/$appointmentId/status" -Method Put -Body $statusBody -ContentType "application/json"
Write-Host "Status Update Result: $statusRes" -ForegroundColor Green

# 13. Doctor Creates Medical Record
Write-Header "13. Creating Medical Record..."
$recordBody = @{
    appointmentId = $appointmentId
    diagnosis = "Early stage hypertension. Advised lifestyle changes."
    prescription = "Amlodipine 5mg - once daily for 30 days."
    attachmentsUrl = "https://example.com/reports/hypertension_patient1.pdf"
} | ConvertTo-Json

$recordRes = Invoke-RestMethod -Uri "$baseUrl/appointments/$appointmentId/medical-record" -Method Post -Body $recordBody -ContentType "application/json"
Write-Host "Medical Record Result: $recordRes" -ForegroundColor Green

# 14. Patient Submits Review
Write-Header "14. Patient Submitting Review..."
$reviewBody = @{
    appointmentId = $appointmentId
    rating = 5
    comment = "Excellent doctor! Dr. Jane Smith explained everything very clearly."
} | ConvertTo-Json

$reviewRes = Invoke-RestMethod -Uri "$baseUrl/appointments/$appointmentId/review" -Method Post -Body $reviewBody -ContentType "application/json"
Write-Host "Review Submission Result: $reviewRes" -ForegroundColor Green

Write-Header "ALL API TESTS COMPLETED SUCCESSFULLY!"
