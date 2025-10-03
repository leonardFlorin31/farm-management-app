# farm management app

Am început acest proiect pentru că îmi place să creez soluții pentru probleme existente și să le programez cât mai bine. Am învățat o mulțime de lucruri în timp ce dezvoltam acest proiect, de la design patterns și arhitecturi de proiecte, pana la creearea unui UI interactiv și a unui backend optimizat. Am învățat cum să aplic S.O.L.I.D corect și concis, precum și concepte importante de OOP, în limbajul principal C#, însă și cu ajutorul .NET framework și Microsoft SQL Server.

Datorită acestui proiect, programarea mea s-a îmbunătățit.

Pagina principală, in care se pot vedea colegii din aceeași organizație, iar bucata cu atribuirea rolului este vizibilă doar pentru "admin".
<img width="1920" height="1031" alt="1" src="https://github.com/user-attachments/assets/eca64aec-665b-45d2-bcf1-e1a47f524658" />

Providerul hărților (2 tipuri de hărți pentru navigare mai ușoară) este Google Maps, iar tot ce ține de Markere, Poligoane, mișcarea, afișarea și salvarea acestora, este programat in C#.
Parcelele din imagine sunt in acele culori pentru a fi identificate mai usor in aceasta prezentare punctele cheie, cum ar fi ca acestea pot fi modificate in timp real din aplicatie prin drag al cerculetelor galbene, cu salvarea imediata in baza de date a noilor coordonate ale poligonului dupa ce se finalizeaza drag-ul. De asemenea, este prezentă o funție de undo/redo cu salvare automată în bază de date a oricărei acțiuni asupra hărții. Bucata care afiseaza informatiile parcelelor poate fi indepartata, modificata ca si dimensiune sau mutata in alta partea a ecranului de catre utilizator. Bara cu drop-down de sus ne permite sa mergem instant pe parcela selectata.
<img width="1920" height="1033" alt="2" src="https://github.com/user-attachments/assets/a44eed9e-b651-4213-8fff-5b187bb4a7bf" />

Bara de drop-down permite selectarea tipului parcelei si modifica toate informatiile de deasupra box-urilor din dreapta aceasteia. O parcelă permite 2 tip-uri de informații pe aceasta simultan (Grâne/Animale).
<img width="1920" height="1034" alt="3" src="https://github.com/user-attachments/assets/75040ddd-ea4e-4a03-b88f-f97cbc248ce0" />

Tabelele sunt actualizate in timp real la adaugarea/stergerea datelor din block-ul de sub acestea. Această pagină nu poate fi acesată de "Angajați", doar Contabil, Manager, etc.
<img width="1920" height="1031" alt="4" src="https://github.com/user-attachments/assets/5b6944f2-0f8f-4992-86ef-10e64b8c211e" />

Task-urile pot fi adaugate doar de o persoana care are un rol de "admin" sau "manager" in organizatie. Task-urile în sine pot fi văzute de cine are atribuit acel Task. Adminul și Managerii vad toate Task-urile și le pot modificarea starea.
<img width="1920" height="1041" alt="5" src="https://github.com/user-attachments/assets/eb450747-7db6-47ec-ac00-7a1f01d3acb2" />
<img width="1920" height="1000" alt="6" src="https://github.com/user-attachments/assets/ad753b60-5b96-44d6-96ed-e4caaf4ce284" />
<img width="1903" height="976" alt="7" src="https://github.com/user-attachments/assets/12109139-7b9e-4e79-a103-ed247bbd8f5a" />
