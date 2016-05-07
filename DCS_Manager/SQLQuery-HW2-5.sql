SELECT Fname, Minit, Lname FROM EMPLOYEE, WORKS_ON WHERE Ssn=Essn
 GROUP BY Fname, Minit, Lname Having Count(Pno) > 2;