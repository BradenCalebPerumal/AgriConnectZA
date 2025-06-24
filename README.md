# AgriConnect

**Name:** Braden Caleb Perumal  
**Student Number:** ST10287165  

---

## Contents
- Introduction  
- Requirements  
- How to Apply  
- Application Overview  
- Architecture  
- Functionality  
- Non-Functional Requirements  
- Change Log  
- FAQ's  
- How to Use  
- Licensing  
- Plugins  
- Credits  
- GitHub Link
- Demonstration Video Link
- Admin Login Credentials  
- References  

---

## 1) Introduction
AgriConnect is an ASP.NET Core MVC web application designed to transform the South African agricultural landscape. It connects Farmers, GreenTech Providers, and Employees into a single ecosystem where agricultural products, sustainable technologies, and user approvals are streamlined through modern digital tools.

---

## 2) Requirements
- .NET 6.0 SDK or higher  
- Microsoft SQL Server (Express or Full)  
- Visual Studio Code  
- C# and Razor Extensions for VS Code  
- Firebase (for authentication)  
- Git (version control)

### Sample `appsettings.json` Configuration:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AgriConnectDb;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 3) How to Apply
1. Clone this repository or download the ZIP.  
2. Open the project folder in Visual Studio Code.  
3. Run `dotnet restore` to restore dependencies.  
4. Update the SQL connection string in `appsettings.json`.  
5. Set up Firebase configuration in your authentication service.  
6. Run the project using:  
   ```
   dotnet run
   ```

---

## 4) Application Overview
**Purpose:**  
To streamline agricultural operations by enabling users to post products, request categories, and manage sustainability solutions—all within a modern, interactive dashboard.

---

## 5) Architecture
AgriConnect follows the Model-View-Controller (MVC) pattern:

- **Models:** Data structures for Users, Products, Categories, etc.  
- **Views:** Razor pages for UI interaction.  
- **Controllers:** Handle request processing and coordinate logic.  

Data is stored in SQL Server, while Firebase manages user authentication.

---

## 6) Functionality

### Farmer:
- Submit product listings  
- Request new categories  
- View and manage personal products  

### GreenTech:
- Post eco-friendly technologies  
- View approved categories  

### Employee:
- Approve or reject category requests  
- View, search, and delete users  
- Monitor product posts  

---

## 7) Non-Functional Requirements
- **Security:** Firebase authentication and secure session handling  
- **Performance:** Optimized queries and responsive UI components  
- **Scalability:** Supports a growing number of users and listings  
- **Reliability:** Works consistently across devices and browsers  
- **Usability:** Clean, modern UI with intuitive navigation  

---

## 8) Change Log
- Category approval system implemented with role-based access  
- Firebase Authentication integrated for secure user management  
- CRUD functionality for Farmer and GreenTech users  
- Employee dashboard with real-time filtering and search  
- UI upgraded with animations and responsive components  

---

## 9) FAQs
**Q1:** How do I register as a Farmer?  
**A1:** Visit the home page, click "Register as Farmer," fill out the enquiry form and wait for approval.

**Q2:** Who approves my category request?  
**A2:** The Employee user reviews and approves categories.

**Q3:** Can I upload images with my product?  
**A3:** Yes, you can upload an image file or paste an image URL.

**Q4:** Why can't I see the dashboard after login?  
**A4:** Ensure your role is approved. If not, contact the Employee admin.

**Q5:** Can one user be both a Farmer and GreenTech provider?  
**A5:** No. Each user account supports only one role.

---

## 10) How to Use
1. Open Visual Studio Code and navigate to the project folder.  
2. Run the following commands:
   ```
   dotnet restore
   dotnet ef database update
   dotnet run
   ```
3. Open your browser and go to `http://localhost:5000`

---

## 11) Licensing
AgriConnect is licensed under the MIT License. You are free to use, modify, and distribute the project with proper credit.

---

## 12) Plugins
- Bootstrap (for styling)  
- Firebase Authentication  
- SweetAlert2 (for interactive alerts)

---

## 13) Credits
This project was created and maintained by **Braden Caleb Perumal (ST10287165)** as part of a capstone module.

---

## 14) GitHub Link
https://github.com/VCWVL/prog7311-poe-CalebPerumal28.git

---
## 15) Demonstration Video Link
[https://github.com/VCWVL/prog7311-poe-CalebPerumal28.git
](https://dlssa-my.sharepoint.com/:v:/g/personal/caleb_dlssa_onmicrosoft_com/EUOaNIhaOqVBlu8r-XHagOkBGvzK5kI-FyRLNxW-lIv4_A)
---
## 16) Admin Login Credentials
- **Email:** anna.agriconnectza@gmail.com 
- **Password:** Employee@1234
  
  ---
## 17) General User Login Credentials
--**Role**: Farmer
- **Email:** ayesha.agriconnectza@gmail.com
- **Password:** Farmer@1234
---
--**Role**: GreenTech
- **Email:** braden.agriconnectza@gmail.com
- **Password:** GreenTech@1234
---
## 18) References
- BroCode. C# Full Course. [YouTube](https://www.youtube.com/watch?v=wxznTygnRfQ)  
- BroCode. C# for Beginners. [YouTube](https://www.youtube.com/watch?v=r3CExhZgZV8)  
- Christensen, M. List of Lists in C#. [StackOverflow](https://stackoverflow.com/questions/12628222/creating-a-list-of-lists-in-c-sharp)  
- GeeksforGeeks. C# Constructors. [https://www.geeksforgeeks.org/c-sharp-constructors/](https://www.geeksforgeeks.org/c-sharp-constructors/)  
- Slayden, G. Convert Emoticons to UTF-32. [StackOverflow](https://stackoverflow.com/questions/44728740/how-to-convert-emoticons-to-its-utf-32-escaped-unicode)
