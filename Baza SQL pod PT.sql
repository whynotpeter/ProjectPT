CREATE DATABASE PTproject;

CREATE TABLE studenci
	(
		ID int primary key,
		imie varchar(255) not null,
		nazwisko varchar(255) not null, 
		email varchar(255),
		aktywny bit default 1,
		indeks int UNIQUE CHECK (indeks>=0)
		);

CREATE TABLE przedmioty
	(
		ID int primary key,
		nazwa varchar(255) not null,
		aktywny bit default 1,
		typ varchar(255) not null,
		kierunek int FOREIGN KEY REFERENCES kierunki(ID)
		--wyklad/lab/projekt
		);

CREATE TABLE kadra
	(
		ID int primary key,
		imie varchar(255) not null,
		nazwisko varchar(255) not null, 
		email varchar(255),
		aktywny bit default 1
		);

CREATE TABLE wydzialy
	(
		ID int primary key,
		nazwa varchar(255) not null,
		);

CREATE TABLE kierunki
	(
		ID int primary key,
		nazwa varchar(255) not null,
		wydzial_id int FOREIGN KEY REFERENCES wydzialy(ID),
		tryb_stacjonarny bit not null,
		rok_rozpoczecia int not null,
		CONSTRAINT kierunek_semestr UNIQUE (nazwa,tryb_stacjonarny,rok_rozpoczecia,wydzial_id)
		);

CREATE TABLE zapisani_na_kierunek
	(
		ID int primary key,
		student_id integer FOREIGN KEY REFERENCES studenci(ID),
		kierunek_id integer FOREIGN KEY REFERENCES kierunki(ID),
		aktywny bit default 1
		)

CREATE TABLE zapisani_na_przedmiot
	(
		ID int primary key,
		student_id int FOREIGN KEY REFERENCES studenci(ID),
		przedmiot_id int FOREIGN KEY REFERENCES przedmioty(ID)
		--jak zabezpieczyc przed duplikatem
		);

CREATE TABLE prowadzacy_przedmiot
	(
		ID int primary key,
		przedmiot_id int FOREIGN KEY REFERENCES przedmioty(ID),
		prowadzacy_id int FOREIGN KEY REFERENCES kadra(ID),
		aktywny bit default 1,
		--poczatek date not null,
		--koniec date not null CHECK (koniec>poczatek)
		);	
		
CREATE TABLE sale
	(
		ID int primary key,
		nr_sali varchar(255) not null,
		);


CREATE TABLE plan_zajec
	(
		ID int primary key,
		sala_id int FOREIGN KEY REFERENCES sale(ID),
		przedmiot_id int FOREIGN KEY REFERENCES przedmioty(ID),
		prowadzacy_id int FOREIGN KEY REFERENCES prowadzacy_przedmiot(ID),
		--poczatek date not null,
		--koniec date not null,
		dzien date,
		odbyte bit default 0
		);

	CREATE TABLE lista_obecnosci
	(
		ID int primary key,
		student_id int FOREIGN KEY REFERENCES studenci(ID),
		plan_id int FOREIGN KEY REFERENCES plan_zajec(ID),
		obecny bit default 0,
		spozniony bit default 0
		--dać triger na to, że spozniony dopiero sie zaznacza jak jest obecny
		);
