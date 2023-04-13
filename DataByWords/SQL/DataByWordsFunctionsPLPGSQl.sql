CREATE TYPE head.padej AS ENUM ('Imen', 'Rod', 'Dat', 'Vin', 'Tvor', 'Pred');
CREATE TYPE head.fractionSpelling AS ENUM ('Off', 'On', 'PartsAsDigits');
CREATE TYPE head.valuta AS ENUM ('Rouble', 'Dollar', 'Euro');
CREATE TYPE head.sex AS ENUM ('Male', 'Female');
CREATE TYPE head.dateFormat AS ENUM ('ShortDate', 'LongDate');

CREATE OR REPLACE FUNCTION head.EndTh(ThNum int, Dek int) 
RETURNS TEXT AS $$
DECLARE 
	flag bool;
	flag2 bool;
BEGIN 
	flag = Dek >= 2 AND Dek <= 4;
	flag2 = Dek > 4 OR Dek = 0;
	IF ThNum > 2 AND flag THEN 
		RETURN 'а';
	END IF;
	IF ThNum = 2 AND Dek = 1 THEN 
		RETURN 'а';
	END IF;
	IF ThNum > 2 AND flag2 THEN 
		RETURN 'ов';
	END IF;
	IF ThNum = 2 AND flag THEN 
		RETURN 'и';
	END IF;

	RETURN null;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.EndDek1(Dek int, IsMale bool) 
RETURNS TEXT AS $$
BEGIN 
	IF Dek <= 2 THEN
		CASE 
			WHEN Dek = 0 THEN
			WHEN Dek = 1 THEN
				IF IsMale THEN  
					RETURN 'ин';
				END IF;
				RETURN 'на';
			ELSE
				IF IsMale THEN  
					RETURN 'а';
				END IF;
				RETURN 'е';
		END CASE;
	END IF;

	RETURN null;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.NumPhrase(value decimal, isMale bool, firstSymbol bool) 
RETURNS TEXT AS $$
DECLARE 
	array1 text[20] = '{
						"",
		                " од",
		                " дв",
		                " три",
		                " четыре",
		                " пять",
		                " шесть",
		                " семь",
		                " восемь",
		                " девять",
		                " десять",
		                " одиннадцать",
		                " двенадцать",
		                " тринадцать",
		                " четырнадцать",
		                " пятнадцать",
		                " шестнадцать",
		                " семнадцать",
		                " восемнадцать",
		                " девятнадцать"
						}';
	array2 text[10] = '{
						"",
		                "",
		                " двадцать",
		                " тридцать",
		                " сорок",
		                " пятьдесят",
		                " шестьдесят",
		                " семьдесят",
		                " восемьдесят",
		                " девяносто"
						}';
	array3 text[10] = '{
						"",
		                " сто",
		                " двести",
		                " триста",
		                " четыреста",
		                " пятьсот",
		                " шестьсот",
		                " семьсот",
		                " восемьсот",
		                " девятьсот"
						}';
	array4 text[8] = '{
						"",
		                "",
		                " тысяч",
		                " миллион",
		                " миллиард",
		                " триллион",
		                " квадрилион",
		                " квинтилион"
						}';
	str TEXT;
	b int;
	b2 int;
	b3 int;
	b4 int;
	num int;
	isCheckMale bool;
BEGIN 
	IF value = 0 THEN
		IF firstSymbol = false THEN
			RETURN 'ноль';
		END IF;
	
		RETURN 'Ноль';
	END IF;
	
	str = '';
	b = 1;
	
	WHILE value != 0
	LOOP	
		num = value % 1000;
		value = (value - num) / 1000;

		IF num > 0 THEN 
			b2 = (num - num % 100) / 100;
            b3 = num % 10;
            b4 = (num - b2 * 100 - b3) / 10;
           
           IF b4 = 1 THEN
           		b3 = b3 + 10;
           END IF;
          
          	isCheckMale = b > 2 OR (b = 1 AND isMale);
          
          	str = concat(array3[b2+1], array2[b4+1], array1[b3+1], head.EndDek1(b3, isCheckMale), array4[b+1], head.EndTh(b, b3), str);
		END IF;
		b = b + 1;
	END LOOP;
	
	IF firstSymbol THEN		
		str = concat(upper(substring(str from 2 for 1)), substring(str from 3));
	END IF;
	RETURN str;
END
$$ LANGUAGE plpgsql;
           
CREATE OR REPLACE FUNCTION head.CurPhrase2(money decimal, word1 TEXT, word234 TEXT, wordmore TEXT, word1_r TEXT, word234_r TEXT, wordmore_r TEXT, word1_d TEXT, word234_d TEXT, wordmore_d TEXT, sword1 TEXT, sword234 TEXT, swordmore TEXT, padeg text, frS text, out one TEXT, out two TEXT, out three TEXT, out four TEXT) 
AS $$
DECLARE 
	num decimal;
	num2 decimal;
	num3 int;
	num4 int;
	str TEXT;
	b int;
	b2 int;
	b3 int;
	trimChars char[1] = '{"0"}';	
BEGIN 
	money = round(money, 2);
	num = trunc(money);
	num2 = num;
	str = concat(head.NumPhrase(num2, true, true), ' ');
	one = btrim(str);
	b = num2 % 100;
	
	padeg = btrim(upper(padeg));
	frS = btrim(upper(frS));
	
	IF b > 19 THEN
		b = b % 10;
	END IF;
	
	CASE
		WHEN padeg = 'ROD' THEN
			CASE
				WHEN b = 1 THEN two = word1_r;
				WHEN b >= 2 AND b <= 4 THEN two = word234_r;
				ELSE two = wordmore_r;
			END CASE;
		WHEN padeg = 'IMEN' THEN
			CASE
				WHEN b = 1 THEN two = word1;
				WHEN b >= 2 AND b <= 4 THEN two = word234;
				ELSE two = wordmore;
			END CASE;
		WHEN padeg = 'DAT' THEN
			CASE
				WHEN b = 1 THEN two = word1_d;
				WHEN b >= 2 AND b <= 4 THEN two = word234_d;
				ELSE two = wordmore_d;
			END CASE;
		ELSE 			
	END CASE;
		
	b2 = (money - num) * 100;
	IF frS != 'PARTSASDIGITS' THEN
		IF b2 != 0 THEN
			num3 = length(btrim((money - num)::TEXT, trimChars[1])) - 1;
			
			IF b2 % 10 = 0 THEN 
				three = head.NumPhrase(b2 / 10, false, false);
			ELSE
				three = head.NumPhrase(b2, false, false);
			END IF;
			
			num4 = rtrim(b2::TEXT, trimChars[1])::int;
			
			IF num3 = 1 THEN 			
				IF num4 = 1 THEN 
					three = concat(three, ' десятая');
				ELSE
					three = concat(three, ' десятых');
				END IF;				
			ELSEIF num4 = 1 THEN 
				three = concat(three, ' сотая');
			ELSE
				three = concat(three, ' сотых');
			END IF;		
		
		ELSEIF frS = 'ON' THEN 
			three = concat(three, ' ноль десятых');
		END IF;
	ELSE		 
		three = b2::text;
		
		IF length(b2::text) = 1 THEN
			three = concat(0, three);
		END IF;
		
		three = concat(' ', three, '/100');	
	END IF;
	IF b2 > 0 OR frS = 'ON' THEN
		CASE
			WHEN padeg = 'ROD' THEN two = wordmore_r;
			WHEN padeg = 'IMEN' THEN two = wordmore;
			WHEN padeg = 'DAT' THEN two = wordmore_d;
			ELSE	
		END CASE;
		
		IF frS != 'PARTSASDIGITS' THEN
			one = head.NumPhrase(num2, false, true);
			b3 = (substring(num2::text from length((num2)::text)))::int;
			IF b3 = 1 AND num2 != 11 THEN 
				one = concat(one, ' целая');
			ELSE 
			 	one = concat(one, ' целых');
			END IF;				
		END IF;
		one = concat(one, ' и');
	ELSEIF b2 = 0 AND frS = 'PARTSASDIGITS' THEN 
		one = concat(one, ' и');
	END IF;

	IF b2 > 19 THEN	
		b2 = b2 % 10;
	END IF;

	CASE
		WHEN b = 1 THEN four = sword1;
		WHEN b >= 2 AND b <= 4 THEN four = sword234;
		ELSE four = swordmore;
	END CASE;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.myDecimalTryParce(value text, OUT valueD decimal)
AS $$
BEGIN 
	BEGIN
		valueD = abs(REPLACE(value, ',', '.')::decimal);		
	EXCEPTION 
		WHEN OTHERS THEN
			BEGIN
				valueD = abs(REPLACE(value, '.', ',')::decimal);		
			EXCEPTION 
				WHEN OTHERS THEN
					valueD = -1;						
			END;
	END;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumberString(value decimal, indexFrS int) 
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	fractionSpellingAr TEXT[6];
BEGIN 
	fractionSpellingAr = enum_range(null::head.fractionSpelling);

	SELECT concat(one, three) INTO STRICT name FROM head.curphrase2(value,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1, enum_first(null::head.padej)::text, fractionSpellingAr[indexFrS]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumberStringWithPadeg(value decimal, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
	fractionSpellingAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);
	fractionSpellingAr = enum_range(null::head.fractionSpelling);
	
	SELECT concat(one, three) INTO STRICT name FROM head.curphrase2(value,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1, padejAr[indexPadeg] ,fractionSpellingAr[indexFrS]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumberString(value text, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;
	
	RETURN head.GetNumbertext(valueD, indexFrS);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumberStringWithPadeg(value text, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetNumbertext(valueD, indexPadeg, indexFrS);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.CurPhrase(money decimal, word1 TEXT, word234 TEXT, wordmore TEXT, word1_r TEXT, word234_r TEXT, wordmore_r TEXT, word1_d TEXT, word234_d TEXT, wordmore_d TEXT, sword1 TEXT, sword234 TEXT, swordmore TEXT, padeg text, allowTwo bool DEFAULT true, out one TEXT, out two TEXT, out three TEXT, out four TEXT) 
AS $$
DECLARE 
	num decimal;
	num2 bigint;
	str TEXT;
	b int;
	b2 int;	
BEGIN 
	money = round(abs(money), 2);
	num = trunc(money);
	num2 = num;
	str = concat(head.NumPhrase(num2, true, true), ' ');
	one = btrim(str);
	b = num2 % 100;
	padeg = btrim(upper(padeg));

	IF b > 19 THEN
		b = b % 10;
	END IF;
	
	CASE
		WHEN padeg = 'ROD' THEN
			CASE
				WHEN b = 1 THEN two = word1_r;
				WHEN b >= 2 AND b <= 4 THEN two = word234_r;
				ELSE two = wordmore_r;
			END CASE;
		WHEN padeg = 'IMEN' THEN
			CASE
				WHEN b = 1 THEN two = word1;
				WHEN b >= 2 AND b <= 4 THEN two = word234;
				ELSE two = wordmore;
			END CASE;
		WHEN padeg = 'DAT' THEN
			CASE
				WHEN b = 1 THEN two = word1_d;
				WHEN b >= 2 AND b <= 4 THEN two = word234_d;
				ELSE two = wordmore_d;
			END CASE;
		ELSE 			
	END CASE;
		
	b2 = (money - num) * 100;

	IF b2 < 10 THEN
		three = concat('0', b2::text);
	ELSE
		three = b2::text;
	END IF;
	
	IF allowTwo THEN
		CASE
			WHEN padeg = 'ROD' THEN
				two = wordmore_r;
			WHEN padeg = 'IMEN' THEN
				two = wordmore;
			WHEN padeg = 'DAT' THEN
				two = wordmore_d;
			ELSE 			
		END CASE;
	END IF;

	IF b2 > 19 THEN	
		b2 = b2 % 10;
	END IF;

	CASE
		WHEN b2 = 1 THEN four = sword1;
		WHEN b2 >= 2 AND b2 <= 4 THEN four = sword234;
		ELSE four = swordmore;
	END CASE;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(',one,' и ', three, '/100) ', two) INTO STRICT name FROM head.CurPhrase(value, 'доллар', 'доллара', 'долларов', 'доллара', 'долларов', 'долларов', 'доллару', 'долларам', 'долларам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetDollarString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(',one,' и ', three, '/100) ', two) INTO STRICT name FROM head.CurPhrase(value, 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetEuroString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(',one,' и ', three, '/100) ', two) INTO STRICT name FROM head.CurPhrase(value, 'рубль', 'рубля', 'рублей', 'рубля', 'рублей', 'рублей', 'рублю', 'рублям', 'рублям', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetRubleString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleStringWithKopeika(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	textKop TEXT;
	padejAr TEXT[6];
	num int;
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat(one, ' ', two, ' ', three) INTO STRICT name FROM head.CurPhrase(value, 'рубль', 'рубля', 'рублей', 'рубля', 'рублей', 'рублей', 'рублю', 'рублям', 'рублям', '', '', '', padejAr[indexPadeg]);
	
	IF POSITION('.' in value::text) > 0 THEN
		num = split_part(value::text, '.', 2)::int;
	ELSE	
		num = 0;
	END IF;	

	textKop = head.GetPraseEnding(num, 'копейка', 'копейки', 'копеек', 'копейку', 'копейки', 'копеек', 'копейке', 'копейкам', 'копейкам', padejAr[indexPadeg]);
	RETURN concat(name, ' ', textKop);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleStringWithKopeika(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetRubleStringWithKopeika(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPraseEnding(value int, rublImen text, rublyaImen text, rubleyImen text, rublyaRod text, rubleyRod text, rubley_Rod text, rubluDat text, rublyamDat text, rublyam_Dat text, padejName text)
RETURNS TEXT AS $$
DECLARE 
	num int;	
BEGIN 
	padejName = upper(padejName);
	num = value % 10;
	CASE
		WHEN padejName = 'IMEN' THEN
			IF value >= 11 AND value <= 19 THEN
				RETURN rubleyImen;
			END IF;
			
			IF num = 1 THEN
				RETURN rublImen;
			END IF;
		
			IF num >= 2 AND num <= 4 THEN
				RETURN rublyaImen;
			END IF;
		
			IF ((num < 5 OR num > 9) AND num != 0)THEN
				RETURN '';
			END IF;
		
			RETURN rubleyImen;
		
		WHEN padejName = 'ROD' THEN
			IF value >= 11 AND value <= 19 THEN
				RETURN rubley_Rod;
			END IF;
			
			IF num = 1 THEN
				RETURN rublyaRod;
			END IF;
		
			IF num >= 2 AND num <= 4 THEN
				RETURN rubleyRod;
			END IF;
		
			IF ((num < 5 OR num > 9) AND num != 0)THEN
				RETURN '';
			END IF;
		
			RETURN rubley_Rod;
		
		WHEN padejName = 'DAT' THEN
			IF value >= 11 AND value <= 19 THEN
				RETURN rublyam_Dat;
			END IF;
			
			IF num = 1 THEN
				RETURN rubluDat;
			END IF;
		
			IF num >= 2 AND num <= 4 THEN
				RETURN rublyamDat;
			END IF;
		
			IF ((num < 5 OR num > 9) AND num != 0)THEN
				RETURN '';
			END IF;
		
			RETURN rublyam_Dat;
		ELSE
	END CASE;
	
	RETURN '';
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleStringWithKopeikaAndBrackets(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	textKop TEXT;
	padejAr TEXT[6];
	num int;
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(', one, ') ', two, ' ', three) INTO STRICT name FROM head.CurPhrase(value, 'рубль', 'рубля', 'рублей', 'рубля', 'рублей', 'рублей', 'рублю', 'рублям', 'рублям', '', '', '', padejAr[indexPadeg], false);
	
	IF POSITION('.' in value::text) > 0 THEN
		num = split_part(value::text, '.', 2)::int;
	ELSE	
		num = 0;
	END IF;	

	textKop = head.GetPraseEnding(num, 'копейка', 'копейки', 'копеек', 'копейку', 'копейки', 'копеек', 'копейке', 'копейкам', 'копейкам', padejAr[indexPadeg]);
	RETURN concat(name, ' ', textKop);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRubleStringWithKopeikaAndBrackets(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetRubleStringWithKopeika(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRRubleString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(',one,' и ', three, '/100) ', two) INTO STRICT name FROM head.CurPhrase(value, 'российский рубль', 'российского рубля', 'российских рублей', 'российского рубля', 'российских рублей', 'российских рублей', 'российскому рублю', 'российским рублям', 'российским рублям', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRRubleString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetRRubleString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(',one,' и ', three, '/100)') INTO STRICT name FROM head.CurPhrase(value, '', '', '', '', '', '', '', '', '', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetNumericString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericStringFS(value decimal, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
	fractionSpellingAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);
	fractionSpellingAr = enum_range(null::head.fractionSpelling);
	
	SELECT concat('(', one, three, ')') INTO STRICT name FROM head.curphrase2(value,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1,empty1, padejAr[indexPadeg] ,fractionSpellingAr[indexFrS]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericStringFS(value text, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetNumericStringFS(valueD, indexPadeg, indexFrS);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericStringWithPrefixNumber(value decimal, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
BEGIN 
	RETURN concat(value, ' (', head.GetNumberStringWithPadeg(value, indexPadeg, indexFrS),')');
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetNumericStringWithPrefixNumber(value text, indexPadeg int, indexFrS int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetNumericStringWithPrefixNumber(valueD, indexPadeg, indexFrS);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDayString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat(one, two) INTO STRICT name FROM head.CurPhrase(value, ' день', ' дня', ' дней', ' дня', ' дней', ' дней', ' дню', ' дням', ' дням', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDayString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetDayString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(', oneD, threeD, ')', twoD) INTO STRICT name FROM head.CurPhraseGeneral(value, ' процент', ' процента', ' процентов', ' процента', ' процентов', ' процентов', ' проценту', ' процентам', ' процентам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetPercentString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.CurPhraseGeneral(money decimal, word1 TEXT, word234 TEXT, wordmore TEXT, word1_r TEXT, word234_r TEXT, wordmore_r TEXT, word1_d TEXT, word234_d TEXT, wordmore_d TEXT, sword1 TEXT, sword234 TEXT, swordmore TEXT, padeg text, out oneD TEXT, out twoD TEXT, out threeD TEXT, out fourD TEXT)
AS $$
DECLARE 
	resultQuery TEXT;
BEGIN 
	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase2(money, word1, word234, wordmore, word1_r, word234_r, wordmore_r, word1_d, word234_d, wordmore_d, sword1, sword234, swordmore, padeg, enum_first(null::head.fractionSpelling)::text);
	oneD = split_part(resultQuery::text, ',', 1);
	twoD = split_part(resultQuery::text, ',', 2);
	threeD = split_part(resultQuery::text, ',', 3);
	fourD = split_part(resultQuery::text, ',', 4);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentStringWithoutAnd(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	padejAr = enum_range(null::head.padej);

	SELECT concat('(', replace(oneD, ' и', ''), threeD, ')', twoD) INTO STRICT name FROM head.CurPhraseGeneral(valueD, ' процент', ' процента', ' процентов', ' процента', ' процентов', ' процентов', ' проценту', ' процентам', ' процентам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentStringDec(value decimal, indexPadeg int, decimals int)
RETURNS TEXT AS $$
DECLARE 
	name TEXT;
	padejAr TEXT[6];
	num decimal;
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('% (', oneD, threeD, ')', twoD) INTO STRICT name FROM head.CurPhraseGeneral(value, ' процент', ' процента', ' процентов', ' процента', ' процентов', ' процентов', ' проценту', ' процентам', ' процентам', '', '', '', padejAr[indexPadeg]);
	
	num = round(value, decimals);

	RETURN concat(num, name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentStringDec(value text, indexPadeg int, decimals int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetPercentStringDec(valueD, indexPadeg, decimals);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentStringWithDecimals(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(', one, ' и ', three, '/100) ', two) INTO STRICT name FROM head.CurPhrase(value, 'процент', 'процента', 'процентов', 'процента', 'процентов', 'процентов', 'проценту', 'процентам', 'процентам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPercentStringWithDecimals(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetPercentStringWithDecimals(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetMonthString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat('(', REPLACE(oneD, ' и', ''), ') ', twoD) INTO STRICT name FROM head.CurPhraseGeneral(value, 'месяц', 'месяца', 'месяцев', 'месяца', 'месяцев', 'месяцев', 'месяцу', 'месяцам', 'месяцам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetMonthString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetMonthString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetCalendarMonthString(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	empty1 TEXT;
	name TEXT;
	padejAr TEXT[6];
BEGIN 
	padejAr = enum_range(null::head.padej);

	SELECT concat(trunc(value), ' (', REPLACE(oneD, ' и', ''), ') ', twoD) INTO STRICT name FROM head.CurPhraseGeneral(value, 'календарный месяц', 'календарных месяца', 'календарных месяцев', 'календарных месяца', 'календарных месяцев', 'календарных месяцев', 'календарному месяцу', 'календарным месяцам', 'календарным месяцам', '', '', '', padejAr[indexPadeg]);
	RETURN name;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetCalendarMonthString(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetCalendarMonthString(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.DateDiffYear(dateOneP text, dateTwoP text)
RETURNS TEXT AS $$
DECLARE 
	dateOne TIMESTAMP;
	dateTwo TIMESTAMP;
	dateThree TIMESTAMP;
	num int;
BEGIN 
	BEGIN
		dateOne = to_date(dateOneP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату первого входного параметра функции!';	
	END;

	BEGIN
		dateTwo = to_date(dateTwoP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату второго входного параметра функции!';	
	END;

	IF dateOne > dateTwo  THEN
		dateThree = dateTwo;
		dateTwo = dateOne;
		dateOne = dateThree;
	END IF;
	
	num = EXTRACT(YEAR FROM dateTwo)::int - EXTRACT(YEAR FROM dateOne)::int;
	
	IF EXTRACT(MONTH FROM dateOne)::int = EXTRACT(MONTH FROM dateTwo)::int THEN
		IF EXTRACT(DAY FROM dateOne)::int < EXTRACT(DAY FROM dateTwo)::int THEN
			RETURN (num - 1)::TEXT;	
		END IF;
	END IF;
	
	IF EXTRACT(MONTH FROM dateOne)::int > EXTRACT(MONTH FROM dateTwo)::int THEN
		RETURN (num - 1)::TEXT;
	END IF;

	RETURN num::TEXT;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.DateDiffMonth(dateOneP text, dateTwoP text)
RETURNS TEXT AS $$
DECLARE 
	dateOne TIMESTAMP;
	dateTwo TIMESTAMP;
	dateThree TIMESTAMP;
	num int;
	num2 int;
	flag bool = FALSE;
BEGIN 
	BEGIN
		dateOne = to_date(dateOneP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату первого входного параметра функции!';	
	END;

	BEGIN
		dateTwo = to_date(dateTwoP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату второго входного параметра функции!';	
	END;

	IF dateOne > dateTwo  THEN
		dateThree = dateTwo;
		dateTwo = dateOne;
		dateOne = dateThree;
	END IF;
	
	num = EXTRACT(YEAR FROM dateTwo)::int - EXTRACT(YEAR FROM dateOne)::int;
	
	IF EXTRACT(MONTH FROM dateOne)::int = EXTRACT(MONTH FROM dateTwo)::int THEN
		IF EXTRACT(DAY FROM dateOne)::int < EXTRACT(DAY FROM dateTwo)::int THEN
			num = num - 1;
			flag = TRUE;
		END IF;
	END IF;
	
	IF EXTRACT(MONTH FROM dateOne)::int > EXTRACT(MONTH FROM dateTwo)::int THEN
		num = num - 1;
		flag = TRUE;
	END IF;

	IF flag THEN
		num2 = 12 - EXTRACT(MONTH FROM dateOne)::int + EXTRACT(MONTH FROM dateTwo)::int;
	ELSE
		num2 = EXTRACT(MONTH FROM dateTwo)::int - EXTRACT(MONTH FROM dateOne)::int;
	END IF;
	
	IF EXTRACT(DAY FROM dateOne)::int > EXTRACT(DAY FROM dateTwo)::int THEN
		num = num - 1;
	END IF;
	
	RETURN (num * 12 + num2)::TEXT;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.DateDiffDay(dateOneP text, dateTwoP text)
RETURNS TEXT AS $$
DECLARE 
	dateOne TIMESTAMP;
	dateTwo TIMESTAMP;
BEGIN 
	BEGIN
		dateOne = to_date(dateOneP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату первого входного параметра функции!';	
	END;

	BEGIN
		dateTwo = to_date(dateTwoP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату второго входного параметра функции!';	
	END;

	RETURN EXTRACT(DAY FROM dateOne::TIMESTAMP without time zone - dateTwo::TIMESTAMP without time zone)::int;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.DateDiffAsString(dateOneP text, dateTwoP text)
RETURNS TEXT AS $$
DECLARE 
	dateOne TIMESTAMP;
	dateTwo TIMESTAMP;
	dateThree TIMESTAMP;
	num int;
	num2 int;
	num3 int;
	num4 int;
	t TIMESTAMP;
	t2 TIMESTAMP;
	textN TEXT; 
BEGIN 
	BEGIN
		dateOne = to_date(dateOneP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату первого входного параметра функции!';	
	END;

	BEGIN
		dateTwo = to_date(dateTwoP, 'YYYY-MM-DD');		
	EXCEPTION 
		WHEN OTHERS THEN
		RETURN 'Не удалось преобразовать в дату второго входного параметра функции!';	
	END;

	IF dateOne <= dateTwo THEN
		dateThree = dateOne;
		t = dateTwo;
	ELSE
		dateThree = dateTwo;
		t = dateOne;
	END IF;
	
	IF head.IsLeapYear(dateThree::timestamp) = false OR head.IsLeapYear(t::timestamp) OR EXTRACT(MONTH FROM dateThree) != 2 OR EXTRACT(DAY FROM dateThree) != 29 THEN
		t2 = to_date(concat(EXTRACT(YEAR FROM t), '-', EXTRACT(MONTH FROM dateThree), '-', EXTRACT(DAY FROM dateThree)), 'YYYY-MM-DD');	
	ELSE
		t2 = to_date(concat(EXTRACT(YEAR FROM t), '-', EXTRACT(MONTH FROM dateThree), '-', EXTRACT(DAY FROM dateThree) - 1), 'YYYY-MM-DD');
	END IF;
	
	IF t2 > t THEN
		num = EXTRACT(YEAR FROM t) - EXTRACT(YEAR FROM dateThree) - 1;
		num2 = EXTRACT(MONTH FROM t) - EXTRACT(MONTH FROM dateThree) + 12 * 1;
	ELSE
		num = EXTRACT(YEAR FROM t) - EXTRACT(YEAR FROM dateThree);
		num2 = EXTRACT(MONTH FROM t) - EXTRACT(MONTH FROM dateThree) + 12 * 0;
	END IF;

	num3 = EXTRACT(DAY FROM t) - EXTRACT(DAY FROM dateThree);

	IF num3 < 0 THEN
		IF EXTRACT(DAY FROM t) = head.GetDaysInMonth(t, EXTRACT(MONTH FROM t)) AND EXTRACT(DAY FROM dateThree) >= head.GetDaysInMonth(t, EXTRACT(MONTH FROM t)) OR EXTRACT(DAY FROM dateThree) >= head.GetDaysInMonth(t, EXTRACT(MONTH FROM dateThree)) THEN
			num3 = 0;
		ELSE
			num2 = num2 - 1;
			IF head.GetDaysInMonth(t, EXTRACT(MONTH FROM t)) = head.GetDaysInMonth(dateThree, EXTRACT(MONTH FROM dateThree)) AND EXTRACT(MONTH FROM t) != EXTRACT(MONTH FROM dateThree) THEN
				IF (EXTRACT(MONTH FROM t) - 1) > 0 THEN
					num4 = head.GetDaysInMonth(t, EXTRACT(MONTH FROM t) - 1);
				ELSE
					num4 = 31;
				END IF;
				num3 = num4 + num3;
			ELSE
				IF EXTRACT(MONTH FROM t) = 1 THEN
					num3 = head.GetDaysInMonth(t, 12) + num3;
				ELSE
					num3 = head.GetDaysInMonth(t, EXTRACT(MONTH FROM t) - 1) + num3;
				END IF;
			END IF;				
		END IF;		
	END IF;
	
	IF num > 0 THEN
		textN = concat(textN, num, 'г. ');
	END IF;

	IF num2 > 0 THEN
		textN = concat(textN, num2, 'мес. ');
	END IF;
	
	IF num3 > 0 THEN
		textN = concat(textN, num3, 'д. ');
	END IF;

	RETURN textN;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDaysInMonth(dateD timestamp, monthN int )
RETURNS int AS $$
BEGIN
	CASE monthN
		WHEN 1 THEN RETURN 31;
		WHEN 2 THEN 
			IF head.IsLeapYear(dateD) THEN
				RETURN 28;
			ELSE
				RETURN 29;
			END IF;
		WHEN 3 THEN RETURN 31;
		WHEN 4 THEN RETURN 30;
		WHEN 5 THEN RETURN 31;
		WHEN 6 THEN RETURN 30;
		WHEN 7 THEN RETURN 31;
		WHEN 8 THEN RETURN 31;
		WHEN 9 THEN RETURN 30;
		WHEN 10 THEN RETURN 31;
		WHEN 11 THEN RETURN 30;
		WHEN 12 THEN RETURN 31;
		ELSE RETURN 31;
	END CASE;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.IsLeapYear(dateD timestamp)
RETURNS boolean AS $$
BEGIN
  return date_part(
    'day',
    make_date(date_part('year', dateD)::int, 3, 1) - '1 day'::interval
  ) = 29;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.CurrencyConverter(decP decimal, fromP int, toP int, dollarRate decimal, euroRate decimal)
RETURNS DECIMAL AS $$
DECLARE 
	d decimal;
	num decimal;
BEGIN 
	d = decP;
	num = euroRate;
	
	CASE
		WHEN toP = 1 THEN
			CASE
				WHEN fromP = 3 THEN
					IF num = 0 THEN						
						RAISE EXCEPTION 'Не задан курс евро %', num;			
					END IF;
					d = decP * num;
				WHEN fromP = 2 THEN
					IF dollarRate = 0 THEN
						RAISE EXCEPTION 'Не задан курс доллара';	
					END IF;
					d = decP * dollarRate;			
				ELSE
			END CASE;
		WHEN toP = 2 THEN
			CASE
				WHEN fromP = 1 THEN
					IF dollarRate = 0 THEN
						RAISE EXCEPTION 'Не задан курс доллара';	
					END IF;
					d = decP / dollarRate;
				WHEN fromP = 3 THEN
					IF num = 0 THEN						
						RAISE EXCEPTION 'Не задан курс евро';			
					END IF;					
					d = decP * num / dollarRate;			
				ELSE
			END CASE;
		WHEN toP = 3 THEN
			CASE
				WHEN fromP = 1 THEN
					IF num = 0 THEN						
						RAISE EXCEPTION 'Не задан курс евро';			
					END IF;
					d = decP / num;
				WHEN fromP = 2 THEN
					IF dollarRate = 0 THEN
						RAISE EXCEPTION 'Не задан курс доллара';	
					END IF;
				
					IF num = 0 THEN
						RAISE EXCEPTION 'Не задан курс евро';	
					END IF;
					d = decP * dollarRate / num;			
				ELSE
			END CASE;
		ELSE	
	END CASE;
	RETURN round(d, 2);	
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetValuta(value decimal, indexPadeg int, indexValuta int)
RETURNS TEXT AS $$
DECLARE 
	resultR TEXT;
	valutaAr TEXT[3];
	valutaName TEXT;
BEGIN 
	valutaAr = enum_range(null::head.valuta);
	valutaName = upper(valutaAr[indexValuta]);
	
	CASE
		WHEN valutaName = 'ROUBLE' THEN
			resultR = head.GetRuble(value, indexPadeg);
		WHEN valutaName = 'DOLLAR' THEN
			resultR = head.GetDollar(value, indexPadeg);
		WHEN valutaName = 'EURO' THEN
			resultR = head.GetEuro(value, indexPadeg);
		ELSE		
	END CASE;

	RETURN resultR;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetValuta(value text, indexPadeg int, indexValuta int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetValuta(valueD, indexPadeg, indexValuta);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetRuble(value decimal, indexValuta int)
RETURNS TEXT AS $$
DECLARE 
	name TEXT;
BEGIN 
	name = replace(concat(value::text, ' '), '.', ',');
	RETURN concat(name, head.GetRubleString(value, indexValuta));
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollar(value decimal, indexValuta int)
RETURNS TEXT AS $$
DECLARE 
	name TEXT;
BEGIN 
	name = replace(concat(value::TEXT, ' '), '.', ',');
	RETURN concat(name, head.GetDollarString(value, indexValuta));
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuro(value decimal, indexValuta int)
RETURNS TEXT AS $$
DECLARE 
	name TEXT;
BEGIN 
	name = replace(concat(value::TEXT, ' '), '.', ',');
	RETURN concat(name, head.GetEuroString(value, indexValuta));
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDateString(datestr text, indexPadeg int, indexFormat int)
RETURNS TEXT AS $$
DECLARE
	resultR TEXT;
	dateFormatAr TEXT[2];
	dateFormatName TEXT;
	padegAr TEXT[6];
	padegName TEXT;
	textN TEXT;
    dayN int;
    monthN int;    
    yearN int;
   	str TEXT;
    emptyN TEXT;
    flag bool = false;
   	dateTime TIMESTAMP;
   	num int;
   	num2 int;
   	num3 int;
   	resultQuery TEXT;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	IF length(trim(datestr)) = 0 THEN
		RETURN '';
	END IF;
	
	padegAr = enum_range(null::head.padej);
	padegName = upper(padegAr[indexPadeg]);
	dateFormatAr = enum_range(null::head.dateFormat);
	dateFormatName = upper(dateFormatAr[indexFormat]);
	
	dateTime = to_date(datestr, 'YYYY-MM-DD');
	yearN = EXTRACT(YEAR FROM dateTime);
	monthN = EXTRACT(MONTH FROM dateTime);
	dayN = EXTRACT(DAY FROM dateTime);
	
	CASE monthN
		WHEN 1 THEN textN = 'января';
		WHEN 2 THEN textN = 'февраля';
		WHEN 3 THEN textN = 'марта';
		WHEN 4 THEN	textN = 'апреля';
		WHEN 5 THEN	textN = 'мая';
		WHEN 6 THEN	textN = 'июня';
		WHEN 7 THEN	textN = 'июля';
		WHEN 8 THEN	textN = 'августа';
		WHEN 9 THEN	textN = 'сентября';
		WHEN 10 THEN textN = 'октября';
		WHEN 11 THEN textN = 'ноября';
		WHEN 12 THEN textN = 'декабря';
		ELSE		
	END CASE;

	CASE dayN
		WHEN 1 THEN str = 'Перв';
		WHEN 2 THEN	str = 'Втор';
		WHEN 3 THEN 
			str = 'Третье';
			flag = true;
		WHEN 4 THEN	str = 'Четверт';
		WHEN 5 THEN str = 'Пят';
		WHEN 6 THEN	str = 'Шест';
		WHEN 7 THEN	str = 'Седьм';
		WHEN 8 THEN	str = 'Восьм';
		WHEN 9 THEN	str = 'Девят';
		WHEN 10 THEN str = 'Десят';
		WHEN 11 THEN str = 'Одиннадцат';
		WHEN 12 THEN str = 'Двенадцат';
		WHEN 13 THEN str = 'Тринадцат';
		WHEN 14 THEN str = 'Четырнадцат';
		WHEN 15 THEN str = 'Пятнадцат';
		WHEN 16 THEN str = 'Шестнадцат';
		WHEN 17 THEN str = 'Семнадцат';
		WHEN 18 THEN str = 'Восемнадцат';
		WHEN 19 THEN str = 'Девятнадцат';
		WHEN 20 THEN str = 'Двадцат';
		WHEN 21 THEN str = 'Двадцать перв';
		WHEN 22 THEN str = 'Двадцать втор';
		WHEN 23 THEN 
			str = 'Двадцать третье';
			flag = true;
		WHEN 24 THEN str = 'Двадцать четверт';
		WHEN 25 THEN str = 'Двадцать пят';
		WHEN 26 THEN str = 'Двадцать шест';
		WHEN 27 THEN str = 'Двадцать седьм';
		WHEN 28 THEN str = 'Двадцать восьм';
		WHEN 29 THEN str = 'Двадцать девят';
		WHEN 30 THEN str = 'Тридцат';
		WHEN 31 THEN str = 'Тридцать перв';
		ELSE		
	END CASE;
	
	SELECT concat(strOut, ',', flagOut) INTO STRICT resultQuery FROM head.AddEnding(str, padegName, flag);
	str = split_part(resultQuery::text, ',', 1);
	flag = split_part(resultQuery::text, ',', 2)::bool;

	CASE yearN
		WHEN 2000 THEN emptyN = 'двухтысячного';
		WHEN 1900 THEN emptyN = 'тысяча девятисотого"';
		ELSE	
			IF substring(yearN::TEXT, 1, 2) = '20' THEN
				emptyN = concat(emptyN, 'две тысячи ');
			ELSE
				emptyN = concat(emptyN, 'тысяча девятьсот ');
			END IF;
		
			num = substring(yearN::TEXT, 3, 2);
			
			CASE substring(yearN::TEXT, 2, 1)::int
				WHEN 1 THEN emptyN = concat(emptyN, 'сто ');
				WHEN 2 THEN emptyN = concat(emptyN, 'двести ');
				WHEN 3 THEN emptyN = concat(emptyN, 'триста ');
				WHEN 4 THEN emptyN = concat(emptyN, 'четыреста ');
				WHEN 5 THEN emptyN = concat(emptyN, 'пятьсот ');
				WHEN 6 THEN emptyN = concat(emptyN, 'шестьсот ');
				WHEN 7 THEN emptyN = concat(emptyN, 'семьсот ');
				WHEN 8 THEN emptyN = concat(emptyN, 'восемьсот ');
				WHEN 9 THEN emptyN = concat(emptyN, 'девятьсот ');
				ELSE		
			END CASE;
		
			IF num >= 1 AND num <= 19 THEN
				emptyN = head.GetDayInDate(emptyN, num);
			END IF;
		
			IF substring(num::TEXT, 2, 2) = '0' THEN
				emptyN = head.GetDayInDate(emptyN, num);
			END IF;
			
			num2 = substring(yearN::TEXT, 3, 1)::int;
			num3 = substring(yearN::TEXT, 4, 1)::int;
			
			CASE num2
				WHEN 2 THEN emptyN = concat(emptyN, 'двадцать ');
				WHEN 3 THEN emptyN = concat(emptyN, 'тридцать ');
				WHEN 4 THEN emptyN = concat(emptyN, 'сорок ');
				WHEN 5 THEN emptyN = concat(emptyN, 'пятьдесят ');
				WHEN 6 THEN emptyN = concat(emptyN, 'шестьдесят ');
				WHEN 7 THEN emptyN = concat(emptyN, 'семьдесят ');
				WHEN 8 THEN emptyN = concat(emptyN, 'восемьдесят ');
				WHEN 9 THEN emptyN = concat(emptyN, 'девяносто ');
				ELSE		
			END CASE;
		
			CASE num3
				WHEN 1 THEN emptyN = concat(emptyN, 'первое');
				WHEN 2 THEN emptyN = concat(emptyN, 'второе');
				WHEN 3 THEN emptyN = concat(emptyN, 'третье');
				WHEN 4 THEN emptyN = concat(emptyN, 'четвертое');
				WHEN 5 THEN emptyN = concat(emptyN, 'пятое');
				WHEN 6 THEN emptyN = concat(emptyN, 'шестое');
				WHEN 7 THEN emptyN = concat(emptyN, 'седьмое');
				WHEN 8 THEN emptyN = concat(emptyN, 'восьмое');
				WHEN 9 THEN emptyN = concat(emptyN, 'девятое');
				ELSE		
			END CASE;
	END CASE;

	IF dateFormatName = 'LONGDATE' THEN
		resultR = concat(str, ' ', textN, ' ', emptyN, ' года');
	ELSE
		resultR = concat(dayN, ' ', textN, ' ', yearN, ' года');
	END IF;

	RETURN resultR;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.AddEnding(strIn TEXT, padeg text, flag bool, OUT strOut TEXT, OUT flagOut bool)
AS $$
DECLARE 
	str TEXT;
BEGIN 	
	CASE upper(padeg)
		WHEN 'IMEN' THEN 
			IF flag THEN
				strOut = concat(strIn, '');
			ELSE	
				strOut = concat(strIn, 'ое');
			END IF;
			
		WHEN 'ROD' THEN 	
			IF flag THEN
				strOut = concat(strIn, 'го');
			ELSE	
				strOut = concat(strIn, 'ого');
			END IF;
		
		WHEN 'DAT' THEN 	
			IF flag THEN
				strOut = concat(strIn, 'му');
			ELSE	
				strOut = concat(strIn, 'ому');
			END IF;
		
		ELSE
	END CASE;
	
	flagOut = FALSE;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDayInDate(strIn TEXT, num int)
RETURNS TEXT AS $$
DECLARE 
	str TEXT;
BEGIN 
	CASE num
		WHEN 1 THEN str = concat(strIn, 'первого');
		WHEN 2 THEN str = concat(strIn, 'второго');
		WHEN 3 THEN str = concat(strIn, 'третьего');
		WHEN 4 THEN str = concat(strIn, 'четвертого');
		WHEN 5 THEN str = concat(strIn, 'пятого');
		WHEN 6 THEN str = concat(strIn, 'шестого');
		WHEN 7 THEN str = concat(strIn, 'седьмого');
		WHEN 8 THEN str = concat(strIn, 'восьмого');
		WHEN 9 THEN str = concat(strIn, 'девятого');
		WHEN 10 THEN str = concat(strIn, 'десятого');
		WHEN 11 THEN str = concat(strIn, 'одиннадцатого');
		WHEN 12 THEN str = concat(strIn, 'двенадцатого');
		WHEN 13 THEN str = concat(strIn, 'тринадцатого');
		WHEN 14 THEN str = concat(strIn, 'четырнадцатого');
		WHEN 15 THEN str = concat(strIn, 'пятнадцатого');
		WHEN 16 THEN str = concat(strIn, 'шестнадцатого');
		WHEN 17 THEN str = concat(strIn, 'семнадцатого');
		WHEN 18 THEN str = concat(strIn, 'восемнадцатого');
		WHEN 19 THEN str = concat(strIn, 'девятнадцатого');
		WHEN 20 THEN str = concat(strIn, 'двадцатого');
		WHEN 30 THEN str = concat(strIn, 'тридцатого');
		WHEN 40 THEN str = concat(strIn, 'сорокового');
		WHEN 50 THEN str = concat(strIn, 'пятидесятого');
		WHEN 60 THEN str = concat(strIn, 'шестидесятого');
		WHEN 70 THEN str = concat(strIn, 'семидесятого');
		WHEN 80 THEN str = concat(strIn, 'восьмидесятого');
		WHEN 90 THEN str = concat(strIn, 'девяностого');
		ELSE
	END CASE;
	RETURN str;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDateStringWithCommas(datestr text)
RETURNS TEXT AS $$
DECLARE
	resultR TEXT;
	textN TEXT;
    dayN int;
    monthN int;    
    yearN int;
    flag bool = false;
   	dateTime TIMESTAMP;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	IF length(trim(datestr)) = 0 THEN
		RETURN '';
	END IF;
	
	dateTime = to_date(datestr, 'YYYY-MM-DD');
	yearN = EXTRACT(YEAR FROM dateTime);
	monthN = EXTRACT(MONTH FROM dateTime);
	dayN = EXTRACT(DAY FROM dateTime);
	
	CASE monthN
		WHEN 1 THEN textN = 'января';
		WHEN 2 THEN textN = 'февраля';
		WHEN 3 THEN textN = 'марта';
		WHEN 4 THEN	textN = 'апреля';
		WHEN 5 THEN	textN = 'мая';
		WHEN 6 THEN	textN = 'июня';
		WHEN 7 THEN	textN = 'июля';
		WHEN 8 THEN	textN = 'августа';
		WHEN 9 THEN	textN = 'сентября';
		WHEN 10 THEN textN = 'октября';
		WHEN 11 THEN textN = 'ноября';
		WHEN 12 THEN textN = 'декабря';
		ELSE		
	END CASE;

	IF dayN < 10 THEN
		resultR = concat('«', 0, dayN, '» ', textN, ' ', yearN);
	ELSE
		resultR = concat('«', dayN, '» ', textN, ' ', yearN);
	END IF;

	RETURN resultR;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDay(datestr text)
RETURNS TEXT AS $$
DECLARE 
	date timestamp;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	IF length(trim(datestr)) = 0 THEN
		RETURN '';
	END IF;
	
	date = to_date(datestr, 'YYYY-MM-DD');	

	RETURN EXTRACT(DAY FROM date)::int;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetMonth(datestr text)
RETURNS TEXT AS $$
DECLARE 
	date timestamp;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	IF length(trim(datestr)) = 0 THEN
		RETURN '';
	END IF;
	
	date = to_date(datestr, 'YYYY-MM-DD');	

	RETURN EXTRACT(MONTH FROM date)::int;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetYear(datestr text)
RETURNS TEXT AS $$
DECLARE 
	date timestamp;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	IF length(trim(datestr)) = 0 THEN
		RETURN '';
	END IF;
	
	date = to_date(datestr, 'YYYY-MM-DD');	

	RETURN EXTRACT(YEAR FROM date)::int;
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.ToCrmDateTime(datestr text)
RETURNS TEXT AS $$
DECLARE 
	array1 TEXT[2];
	date timestamp;
	timeZone TEXT;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
		
	date = to_date(datestr, 'yyyy-MM-ddThh24:mi:ss');	
	array1 = string_to_array(CURRENT_TIME::text, '+');

	IF array_length(array1, 1) > 1 THEN
		timeZone = concat('+', array1[2]);
	ELSE
		array1 = string_to_array(CURRENT_TIME::text, '-');
		timeZone = concat('-', array1[2]);
	END IF;	
	
	RETURN concat(date, ' ', timeZone, ':00');
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormatCrmDateTime(crmDateTime TEXT, format TEXT)
RETURNS TEXT AS $$
DECLARE 
	date timestamp;
	array1 TEXT[10];
	array2 TEXT[10];
	array3 TEXT[10];
BEGIN 
	
	IF crmDateTime = '' THEN
		RETURN '';
	END IF;
	
	array1 = string_to_array(crmDateTime::text, 'T');
	array2 = string_to_array(array1[1], '-');
		
	IF array_length(array1, 1) > 1 THEN
		array3 = string_to_array(array1[2], ':');
		date = to_date(concat(array2[1]::int, '-', array2[2]::int, '-', array2[3]::int, 'T', array3[1]::int, ':', array3[2]::int), 'yyyy-MM-ddThh24:mi:ss');	
	ELSE
		date = to_date(concat(array2[1]::int, '-', array2[2]::int, '-', array2[3]::int), 'yyyy-MM-dd');	
	END IF;	

	RETURN to_char(date, format);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormatDate(datestr TEXT, format text)
RETURNS TEXT AS $$
DECLARE 
	date timestamp;
BEGIN 
	
	IF datestr = '' THEN
		RETURN '';
	END IF;
	
	date = to_date(datestr, 'yyyy-mm-dd hh24:mi:ss')::timestamp;

	RETURN to_char(date, format);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormationSurname(lastname TEXT, firstname TEXT, surname TEXT, name TEXT, otchestvo TEXT, pol TEXT, a TEXT, a2 bool, flag bool, 
												OUT fam_rP TEXT, OUT fam_dP TEXT, OUT fam_tP TEXT, OUT fam_vP TEXT, OUT fam_pP TEXT,
												OUT nam_rP TEXT, OUT nam_dP TEXT, OUT nam_tP TEXT, OUT nam_vP TEXT, OUT nam_pP TEXT,
												OUT otch_rP TEXT, OUT otch_dP TEXT, OUT otch_tP TEXT, OUT otch_vP TEXT, OUT otch_pP TEXT)
AS $$
DECLARE 
	resultQuery TEXT;
	num int = 1;
	num2 int = 1;
	a3 bool;
	checkA bool;
	array1 TEXT[10] = '{ "а", "е", "ё", "и", "о", "у", "ы", "э", "ю", "я" }';
	array2 TEXT[10] = '{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "0"}';
	arrayOutData TEXT[15];
BEGIN 
	
	IF surname != '' AND length(surname) > 2 AND flag = false THEN
		WHILE num <= 10 LOOP
			checkA = substring(surname, length(surname), length(surname)) != array1[num];
		    IF checkA AND strpos(surname, array1[num]) != length(surname) THEN
		    	num = num + 1;
		    	CONTINUE;		    
		    END IF;
		    a2 = true;
		   EXIT;
		   
		END LOOP;
		
		a3 = false;
		
		WHILE num2 <= 10 LOOP
		    IF strpos(surname, array2[num2]) != length(surname) THEN
		    	num2 = num2 + 1;
		    	CONTINUE;
		    END IF;
		    a3 = true;
		   	EXIT;
		END LOOP;
	
		IF (a2 = false OR (checkA = false OR strpos(surname, 'а') = length(surname)) OR strpos(surname, 'ая') = length(surname) - 1) AND a3 = false AND (a2 = true OR (strpos(surname, 'ь') != length(surname) AND strpos(surname, 'их') != length(surname) - 1 AND strpos(surname, 'ых') != length(surname) - 1)) THEN
			IF a2 = true AND (checkA = false OR strpos(surname, 'а') = length(surname)) THEN
				
				fam_rP = concat(substring(surname, 0, length(surname)) , 'ой');
				fam_dP = concat(substring(surname, 0, length(surname)) , 'ой');
				fam_tP = concat(substring(surname, 0, length(surname)) , 'ой');
				fam_vP = concat(substring(surname, 0, length(surname)) , 'у');
				fam_pP = concat(substring(surname, 0, length(surname)) , 'ой');
			
			END IF;
		
			SELECT concat(fam_rP2, ',', fam_dP2, ',', fam_tP2, ',', fam_vP2, ',', fam_pP2, ',', nam_rP2, ',', nam_dP2, ',', nam_tP2, ',', nam_vP2, ',', nam_pP2, ',', otch_rP2, ',', otch_dP2, ',', otch_tP2, ',', otch_vP2, ',', otch_pP2) INTO STRICT resultQuery FROM head.FormationSurname2(lastname, firstname, surname, name, otchestvo, pol, a, a2, a3, flag); --goto IL_0460;
			arrayOutData = string_to_array(resultQuery::text, ',');
		
			IF fam_rP IS NULL OR fam_rP = surname THEN				
				fam_rP = arrayOutData[1];
				fam_dP = arrayOutData[2];
				fam_tP = arrayOutData[3];
				fam_vP = arrayOutData[4];
				fam_pP = arrayOutData[5];
			END IF;	
				
			nam_rP = arrayOutData[6];
			nam_dP = arrayOutData[7];
			nam_tP = arrayOutData[8];
			nam_vP = arrayOutData[9];
			nam_pP = arrayOutData[10];
			otch_rP = arrayOutData[11];
			otch_dP = arrayOutData[12];
			otch_tP = arrayOutData[13];
			otch_vP = arrayOutData[14];
			otch_pP = arrayOutData[15];
		
			RETURN;
		END IF;
	
		fam_rP = surname;
		fam_dP = surname;
		fam_tP = surname;
		fam_vP = surname;
		fam_pP = surname;
	
		SELECT concat(fam_rP2, ',', fam_dP2, ',', fam_tP2, ',', fam_vP2, ',', fam_pP2, ',', nam_rP2, ',', nam_dP2, ',', nam_tP2, ',', nam_vP2, ',', nam_pP2, ',', otch_rP2, ',', otch_dP2, ',', otch_tP2, ',', otch_vP2, ',', otch_pP2) INTO STRICT resultQuery FROM head.FormationSurname2(lastname, firstname, surname, name, otchestvo, pol, a, a2, a3, flag); --goto IL_0460;
		arrayOutData = string_to_array(resultQuery::text, ',');
		
		IF arrayOutData[1] != '' AND arrayOutData[1] != lastname THEN
			fam_rP = arrayOutData[1];
			fam_dP = arrayOutData[2];
			fam_tP = arrayOutData[3];
			fam_vP = arrayOutData[4];
			fam_pP = arrayOutData[5];
		END IF;
	
		nam_rP = arrayOutData[6];
		nam_dP = arrayOutData[7];
		nam_tP = arrayOutData[8];
		nam_vP = arrayOutData[9];
		nam_pP = arrayOutData[10];
		otch_rP = arrayOutData[11];
		otch_dP = arrayOutData[12];
		otch_tP = arrayOutData[13];
		otch_vP = arrayOutData[14];
		otch_pP = arrayOutData[15];
		
		RETURN;
	END IF;

	fam_rP = surname;
	fam_dP = surname;
	fam_tP = surname;
	fam_vP = surname;
	fam_pP = surname;

	SELECT concat(nam_rP3, ',', nam_dP3, ',', nam_tP3, ',', nam_vP3, ',', nam_pP3, ',', otch_rP3, ',', otch_dP3, ',', otch_tP3, ',', otch_vP3, ',', otch_pP3) INTO STRICT resultQuery FROM head.FormationName(lastname, firstname, surname, name, otchestvo, pol, a, a2, flag); --goto IL_0b2e;
	arrayOutData = string_to_array(resultQuery::text, ',');
	
	nam_rP = arrayOutData[1];
	nam_dP = arrayOutData[2];
	nam_tP = arrayOutData[3];
	nam_vP = arrayOutData[4];
	nam_pP = arrayOutData[5];
	otch_rP = arrayOutData[6];
	otch_dP = arrayOutData[7];
	otch_tP = arrayOutData[8];
	otch_vP = arrayOutData[9];
	otch_pP = arrayOutData[10];
	
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormationSurname2(lastname TEXT, firstname TEXT, surname TEXT, name TEXT, otchestvo TEXT, pol TEXT, a TEXT, a2 bool, a3 bool, flag BOOL,
												OUT fam_rP2 TEXT, OUT fam_dP2 TEXT, OUT fam_tP2 TEXT, OUT fam_vP2 TEXT, OUT fam_pP2 TEXT,
												OUT nam_rP2 TEXT, OUT nam_dP2 TEXT, OUT nam_tP2 TEXT, OUT nam_vP2 TEXT, OUT nam_pP2 TEXT,
												OUT otch_rP2 TEXT, OUT otch_dP2 TEXT, OUT otch_tP2 TEXT, OUT otch_vP2 TEXT, OUT otch_pP2 TEXT)
AS $$
DECLARE 
	resultQuery TEXT;
	arrayOutData TEXT[10];
BEGIN
	IF a2 = true AND strpos(surname, 'ая') = length(surname) - 1 THEN
		fam_rP2 = concat(substring(surname, 0, length(surname) - 1) , 'ой');
		fam_dP2 = concat(substring(surname, 0, length(surname) - 1) , 'ой');
		fam_tP2 = concat(substring(surname, 0, length(surname) - 1) , 'ой');
		fam_vP2 = concat(substring(surname, 0, length(surname) - 1) , 'ую');
		fam_pP2 = concat(substring(surname, 0, length(surname) - 1) , 'ой');
	END IF;

	IF a2 = false AND strpos(surname, 'ч') = length(surname) - 1 THEN
		fam_rP2 = concat(surname, 'а');
		fam_dP2 = concat(surname, 'у');
		fam_tP2 = concat(surname, 'ем');
		fam_vP2 = concat(surname, 'а');
		fam_pP2 = concat(surname, 'е');
	END IF;

	IF a2 = false AND strpos(surname, 'ий') = length(surname) - 1 THEN
		fam_rP2 = concat(substring(surname, 0, length(surname) - 1) , 'ого');
		fam_dP2 = concat(substring(surname, 0, length(surname) - 1) , 'ому');
		fam_tP2 = concat(substring(surname, 0, length(surname) - 1) , 'им');
		fam_vP2 = concat(substring(surname, 0, length(surname) - 1) , 'ого');
		fam_pP2 = concat(substring(surname, 0, length(surname) - 1) , 'ом');
	END IF;

	IF a2 = false AND (strpos(surname, 'ый') = length(surname) - 1 OR strpos(surname, 'ой') = length(surname) - 1) THEN
		fam_rP2 = concat(substring(surname, 0, length(surname) - 1) , 'ого');
		fam_dP2 = concat(substring(surname, 0, length(surname) - 1) , 'ому');
		fam_tP2 = concat(substring(surname, 0, length(surname) - 1) , 'им');
		fam_vP2 = concat(substring(surname, 0, length(surname) - 1) , 'ого');
		fam_pP2 = concat(substring(surname, 0, length(surname) - 1) , 'ом');
	END IF;

	IF a2 = false AND (strpos(surname, 'ов') = length(surname) - 1 OR strpos(surname, 'ин') = length(surname) - 1 OR strpos(surname, 'ев') = length(surname) - 1) THEN
		fam_rP2 = concat(surname, 'а');
		fam_dP2 = concat(surname, 'у');
		fam_tP2 = concat(surname, 'ым');
		fam_vP2 = concat(surname, 'а');
		fam_pP2 = concat(surname, 'е');
	END IF;

	IF a2 = false AND strpos(surname, 'ч') != length(surname) AND strpos(surname, 'ий') != length(surname) - 1 
		AND strpos(surname, 'ый') != length(surname) - 1 AND strpos(surname, 'ой') != length(surname) - 1 
		AND strpos(surname, 'ь') != length(surname) AND strpos(surname, 'их') != length(surname) - 1 
		AND strpos(surname, 'ых') != length(surname) - 1 AND strpos(surname, 'ов') != length(surname) - 1 
		AND strpos(surname, 'ин') != length(surname) - 1 AND strpos(surname, 'ев') != length(surname) - 1 AND a3 = false THEN		
		fam_rP2 = concat(surname, 'а');
		fam_dP2 = concat(surname, 'у');
		fam_tP2 = concat(surname, 'ом');
		fam_vP2 = concat(surname, 'а');
		fam_pP2 = concat(surname, 'е');
	END IF;

	SELECT concat(nam_rP3, ',', nam_dP3, ',', nam_tP3, ',', nam_vP3, ',', nam_pP3, ',', otch_rP3, ',', otch_dP3, ',', otch_tP3, ',', otch_vP3, ',', otch_pP3) INTO STRICT resultQuery FROM head.FormationName(lastname, firstname, surname, name, otchestvo, pol, a, a2, flag); --goto IL_0b2e;
	arrayOutData = string_to_array(resultQuery::text, ',');
	
	nam_rP2 = arrayOutData[1];
	nam_dP2 = arrayOutData[2];
	nam_tP2 = arrayOutData[3];
	nam_vP2 = arrayOutData[4];
	nam_pP2 = arrayOutData[5];
	otch_rP2 = arrayOutData[6];
	otch_dP2 = arrayOutData[7];
	otch_tP2 = arrayOutData[8];
	otch_vP2 = arrayOutData[9];
	otch_pP2 = arrayOutData[10];

END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormationName(lastname TEXT, firstname TEXT, surname TEXT, name TEXT, otchestvo TEXT, pol TEXT, a TEXT, a2 bool, flag BOOL,
												OUT nam_rP3 TEXT, OUT nam_dP3 TEXT, OUT nam_tP3 TEXT, OUT nam_vP3 TEXT, OUT nam_pP3 TEXT,
												OUT otch_rP3 TEXT, OUT otch_dP3 TEXT, OUT otch_tP3 TEXT, OUT otch_vP3 TEXT, OUT otch_pP3 TEXT)
AS $$
DECLARE 
	resultQuery TEXT;
	num3 int = 1;
	num4 int = 1;
	a4 bool;
	checkA bool;
	array1 TEXT[10] = '{ "а", "е", "ё", "и", "о", "у", "ы", "э", "ю", "я" }';
	array2 TEXT[10] = '{ "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" }';
	arrayOutData TEXT[15];
BEGIN 
		
	IF name != '' AND length(name) > 2 AND flag = false THEN
		WHILE num3 <= 10 LOOP
			checkA = substring(name, length(name), length(name)) != array1[num3];
		    IF checkA AND strpos(name, array1[num3]) != length(name) THEN
		    	num3 = num3 + 1;
		    	CONTINUE;
		    END IF;
		    a2 = true;
		   EXIT;
		END LOOP;
		
		a4 = false;
		
		WHILE num4 <= 10 LOOP
		    IF strpos(name, array2[num4]) != length(name) THEN
		    	num4 = num4 + 1;
		    	CONTINUE;
		    END IF;
		    a4 = true;
		   EXIT;
		END LOOP;
	
		IF a2 = true AND (checkA = false OR strpos(name, 'а') = length(name)) AND strpos(name, 'ка') != length(name) - 1 THEN
			nam_rP3 = concat(substring(name, 0, length(name)) , 'ы');
			nam_dP3 = concat(substring(name, 0, length(name)) , 'е');
			nam_tP3 = concat(substring(name, 0, length(name)) , 'ой');
			nam_vP3 = concat(substring(name, 0, length(name)) , 'у');
			nam_pP3 = concat(substring(name, 0, length(name)) , 'е');
		
		ELSIF a2 = true AND strpos(name, 'ка') = length(name) - 1 THEN
			nam_rP3 = concat(substring(name, 0, length(name)) , 'и');
			nam_dP3 = concat(substring(name, 0, length(name)) , 'е');
			nam_tP3 = concat(substring(name, 0, length(name)) , 'ой');
			nam_vP3 = concat(substring(name, 0, length(name)) , 'у');
			nam_pP3 = concat(substring(name, 0, length(name)) , 'е');
		
		ELSIF a2 = true AND strpos(name, 'я') = length(name) AND strpos(name, 'ия') != length(name) - 1 THEN
			nam_rP3 = concat(substring(name, 0, length(name)) , 'и');
			nam_dP3 = concat(substring(name, 0, length(name)) , 'е');
			nam_tP3 = concat(substring(name, 0, length(name)) , 'ей');
			nam_vP3 = concat(substring(name, 0, length(name)) , 'ю');
			nam_pP3 = concat(substring(name, 0, length(name)) , 'ие');
		
		ELSE
			IF a2 = true AND strpos(name, 'а') != length(name) - 1 AND strpos(name, 'я') != length(name) - 1  THEN
				--IL_0f6b
				nam_rP3 = name;
				nam_dP3 = name;
				nam_tP3 = name;
				nam_vP3 = name;
				nam_pP3 = name;
				
				--IL_13a9
				IF a2 = false AND strpos(name, 'ий') = length(name) - 1 THEN
					nam_pP3 = concat(substring(name, 0, length(name)) , 'и');
				END IF;
			
				SELECT concat(nam_rP4, ',', otch_rP4, ',', otch_dP4, ',', otch_tP4, ',', otch_vP4, ',', otch_pP4) INTO STRICT resultQuery FROM head.FormationOtchestvo(surname, name, otchestvo, pol, a, flag); --goto IL_143f
				arrayOutData = string_to_array(resultQuery::text, ',');
					
				IF firstname = 'Ольга' THEN
					nam_rP3 = arrayOutData[1];
				END IF;
			
				otch_rP3 = arrayOutData[2];
				otch_dP3 = arrayOutData[3];
				otch_tP3 = arrayOutData[4];
				otch_vP3 = arrayOutData[5];
				otch_pP3 = arrayOutData[6];
			
				RETURN;
			END IF;
			
			IF a4 = true THEN
				--IL_0f6b
				nam_rP3 = name;
				nam_dP3 = name;
				nam_tP3 = name;
				nam_vP3 = name;
				nam_pP3 = name;
				
				--IL_13a9
				IF a2 = false AND strpos(name, 'ий') = length(name) - 1 THEN
					nam_pP3 = concat(substring(name, 0, length(name)) , 'и');
				END IF;
			
				SELECT concat(nam_rP4, ',', otch_rP4, ',', otch_dP4, ',', otch_tP4, ',', otch_vP4, ',', otch_pP4) INTO STRICT resultQuery FROM head.FormationOtchestvo(surname, name, otchestvo, pol, a, flag); --goto IL_143f
				arrayOutData = string_to_array(resultQuery::text, ',');
					
				IF firstname = 'Ольга' THEN
					nam_rP3 = arrayOutData[1];
				END IF;
			
				otch_rP3 = arrayOutData[2];
				otch_dP3 = arrayOutData[3];
				otch_tP3 = arrayOutData[4];
				otch_vP3 = arrayOutData[5];
				otch_pP3 = arrayOutData[6];
			
				RETURN;	
			END IF;
		
			IF a2 = false AND (strpos(name, 'й') = length(name) OR strpos(name, 'рь') = length(name) - 1) THEN
				nam_rP3 = concat(substring(name, 0, length(name)) , 'я');
				nam_dP3 = concat(substring(name, 0, length(name)) , 'ю');
				nam_tP3 = concat(substring(name, 0, length(name)) , 'ем');
				nam_vP3 = concat(substring(name, 0, length(name)) , 'я');
				nam_pP3 = concat(substring(name, 0, length(name)) , 'е');
			END IF;
			
			IF a2 = false AND strpos(name, 'ь') = length(name) AND strpos(name, 'рь') != length(name) - 1 THEN
				nam_rP3 = concat(substring(name, 0, length(name)) , 'и');
				nam_dP3 = concat(substring(name, 0, length(name)) , 'и');
				nam_tP3 = concat(substring(name, 0, length(name)) , 'ей');
				nam_vP3 = name;
				nam_pP3 = concat(substring(name, 0, length(name)) , 'е');
			END IF;
		
			IF a2 = true AND strpos(name, 'ия') = length(name) - 1 THEN
				nam_rP3 = concat(substring(name, 0, length(name)) , 'и');
				nam_dP3 = concat(substring(name, 0, length(name)) , 'и');
				nam_tP3 = concat(substring(name, 0, length(name)) , 'ей');
				nam_vP3 = concat(substring(name, 0, length(name)) , 'ю');
				nam_pP3 = concat(substring(name, 0, length(name)) , 'и');
			END IF;
			
			IF a2 = false AND strpos(name, 'ь') != length(name) AND strpos(name, 'й') != length(name) AND a4 = false THEN
				nam_rP3 = concat(name, 'а');
				nam_dP3 = concat(name, 'у');
				nam_tP3 = concat(name, 'ом');
				nam_vP3 = concat(name, 'а');
				nam_pP3 = concat(name, 'е');
			END IF;
		END IF;
	
		--IL_13a9
		IF a2 = false AND strpos(name, 'ий') = length(name) - 1 THEN
			nam_pP3 = concat(substring(name, 0, length(name)) , 'и');
		END IF;
		
		SELECT concat(nam_rP4, ',', otch_rP4, ',', otch_dP4, ',', otch_tP4, ',', otch_vP4, ',', otch_pP4) INTO STRICT resultQuery FROM head.FormationOtchestvo(surname, name, otchestvo, pol, a, flag); --goto IL_143f
		arrayOutData = string_to_array(resultQuery::text, ',');
		
		IF firstname = 'Ольга' THEN
			nam_rP3 = arrayOutData[1];
		END IF;
	
		otch_rP3 = arrayOutData[2];
		otch_dP3 = arrayOutData[3];
		otch_tP3 = arrayOutData[4];
		otch_vP3 = arrayOutData[5];
		otch_pP3 = arrayOutData[6];

		RETURN;	
	END IF;
		
	nam_rP3 = name;
	nam_dP3 = name;
	nam_tP3 = name;
	nam_vP3 = name;
	nam_pP3 = name;

	SELECT concat(nam_rP4, ',', otch_rP4, ',', otch_dP4, ',', otch_tP4, ',', otch_vP4, ',', otch_pP4) INTO STRICT resultQuery FROM head.FormationOtchestvo(surname, name, otchestvo, pol, a, flag); --goto IL_143f
	arrayOutData = string_to_array(resultQuery::text, ',');
		
	IF firstname = 'Ольга' THEN
					nam_rP3 = arrayOutData[1];
				END IF;

	otch_rP3 = arrayOutData[2];
	otch_dP3 = arrayOutData[3];
	otch_tP3 = arrayOutData[4];
	otch_vP3 = arrayOutData[5];
	otch_pP3 = arrayOutData[6];
	
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.FormationOtchestvo(surname TEXT, name TEXT, otchestvo TEXT, pol TEXT, a TEXT, flag bool,
													OUT nam_rP4 TEXT, OUT otch_rP4 TEXT, OUT otch_dP4 TEXT, OUT otch_tP4 TEXT, OUT otch_vP4 TEXT, OUT otch_pP4 TEXT)
AS $$
DECLARE 
	resultQuery TEXT;
	a2 bool;
	num5 int = 1;
	array1 TEXT[10] = '{ "а", "е", "ё", "и", "о", "у", "ы", "э", "ю", "я" }';
BEGIN 
	
	IF otchestvo != '' AND length(otchestvo) <= 2 OR flag = true THEN
		otch_rP4 = otchestvo;
		otch_dP4 = otchestvo;
		otch_tP4 = otchestvo;
		otch_vP4 = otchestvo;
		otch_pP4 = otchestvo;
	
	ELSIF strpos(otchestvo, 'на') = length(otchestvo) - 1 THEN
		otch_rP4 = concat(substring(otchestvo, 0, length(otchestvo)) , 'ы');
		otch_dP4 = concat(substring(otchestvo, 0, length(otchestvo)) , 'е');
		otch_tP4 = concat(substring(otchestvo, 0, length(otchestvo)) , 'ой');
		otch_vP4 = concat(substring(otchestvo, 0, length(otchestvo)) , 'у');
		otch_pP4 = concat(substring(otchestvo, 0, length(otchestvo)) , 'е');	
	
	ELSIF strpos(otchestvo, 'ич') = length(otchestvo) - 1 THEN
		otch_rP4 = concat(otchestvo, 'а');
		otch_dP4 = concat(otchestvo, 'у');
		otch_tP4 = concat(otchestvo, 'ем');
		otch_vP4 = concat(otchestvo, 'а');
		otch_pP4 = concat(otchestvo, 'е');
	
	ELSE
		otch_rP4 = otchestvo;
		otch_dP4 = otchestvo;
		otch_tP4 = otchestvo;
		otch_vP4 = otchestvo;
		otch_pP4 = otchestvo;
	
	END IF;
	
	IF pol != '' AND surname != '' AND pol = '2' THEN
		a2 = false;
		
		WHILE num5 <= 10 LOOP
		    IF strpos(name, array1[num5]) != length(name) THEN
		    	num5 = num5 + 1;
		    	CONTINUE;
		    END IF;
		    a2 = true;
		   EXIT;
		END LOOP;
		
		IF a = 'Ольга' THEN
			nam_rP4 = 'Ольги';
		END IF;		
	
	END IF;	
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarStringWithCent(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	dollar_str TEXT;
	dollar_name TEXT;
	cent_str TEXT;
	cent_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'доллар', 'доллара', 'долларов', 'доллара', 'долларов', 'долларов', 'доллару', 'долларам', 'долларам', '', '', '', padejAr[indexPadeg]);
	
	dollar_str = split_part(resultQuery::text, ',', 1);
	dollar_name = split_part(resultQuery::text, ',', 2);
	cent_str = split_part(resultQuery::text, ',', 3);
	cent_name = split_part(resultQuery::text, ',', 4);

	cent_name = head.GetPraseEnding(fract_value::int, 'цент', 'цента', 'центов', 'цента', 'центов', 'центов', 'центу', 'центам', 'центам', padejAr[indexPadeg]);
	RETURN concat(dollar_str, ' ', dollar_name, ' ', fract_value, ' ', cent_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarStringWithCent(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetDollarStringWithCent(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarStringWithCentAndBrackets(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	dollar_str TEXT;
	dollar_name TEXT;
	cent_str TEXT;
	cent_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'доллар', 'доллара', 'долларов', 'доллара', 'долларов', 'долларов', 'доллару', 'долларам', 'долларам', '', '', '', padejAr[indexPadeg]);
	
	dollar_str = split_part(resultQuery::text, ',', 1);
	dollar_name = split_part(resultQuery::text, ',', 2);
	cent_str = split_part(resultQuery::text, ',', 3);
	cent_name = split_part(resultQuery::text, ',', 4);

	cent_name = head.GetPraseEnding(fract_value::int, 'цент', 'цента', 'центов', 'цента', 'центов', 'центов', 'центу', 'центам', 'центам', padejAr[indexPadeg]);
	RETURN concat('(', dollar_str, ') ', dollar_name, ' ', fract_value, ' ', cent_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetDollarStringWithCentAndBrackets(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetDollarStringWithCentAndBrackets(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroStringWithEurocent(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	evro_str TEXT;
	evro_name TEXT;
	cent_str TEXT;
	cent_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', '', '', '', padejAr[indexPadeg]);
	
	evro_str = split_part(resultQuery::text, ',', 1);
	evro_name = split_part(resultQuery::text, ',', 2);
	cent_str = split_part(resultQuery::text, ',', 3);
	cent_name = split_part(resultQuery::text, ',', 4);

	cent_name = head.GetPraseEnding(fract_value::int, 'евроцент', 'евроцента', 'евроцентов', 'евроцента', 'евроцентов', 'евроцентов', 'евроценту', 'евроцентам', 'евроцентам', padejAr[indexPadeg]);
	RETURN concat(evro_str, ' ', evro_name, ' ', fract_value, ' ', cent_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroStringWithEurocent(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetEuroStringWithEurocent(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroStringWithEurocentAndBrackets(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	evro_str TEXT;
	evro_name TEXT;
	cent_str TEXT;
	cent_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', 'евро', '', '', '', padejAr[indexPadeg]);
	
	evro_str = split_part(resultQuery::text, ',', 1);
	evro_name = split_part(resultQuery::text, ',', 2);
	cent_str = split_part(resultQuery::text, ',', 3);
	cent_name = split_part(resultQuery::text, ',', 4);

	cent_name = head.GetPraseEnding(fract_value::int, 'евроцент', 'евроцента', 'евроцентов', 'евроцента', 'евроцентов', 'евроцентов', 'евроценту', 'евроцентам', 'евроцентам', padejAr[indexPadeg]);
	RETURN concat('(', evro_str, ') ', evro_name, ' ', fract_value, ' ', cent_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetEuroStringWithEurocentAndBrackets(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetEuroStringWithEurocentAndBrackets(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPoundsterlingStringWithPerry(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	pound_str TEXT;
	pound_name TEXT;
	perry_str TEXT;
	perry_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'фунт стерлингов', 'фунта стерлингов', 'фунтов стерлингов', 'фунта стерлингов', 'фунтов стерлингов', 'фунтов стерлингов', 'фунту стерлингов', 'фунтам стерлингов', 'фунтам стерлингов', '', '', '', padejAr[indexPadeg]);
	
	pound_str = split_part(resultQuery::text, ',', 1);
	pound_name = split_part(resultQuery::text, ',', 2);
	perry_str = split_part(resultQuery::text, ',', 3);
	perry_name = split_part(resultQuery::text, ',', 4);

	perry_name = head.GetPraseEnding(fract_value::int, 'пенс', 'пенса', 'пенсов', 'пенса', 'пенсов', 'пенсов', 'пенсу', 'пенсам', 'пенсам', padejAr[indexPadeg]);
	RETURN concat(pound_str, ' ', pound_name, ' ', fract_value, ' ', perry_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPoundsterlingStringWithPerry(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetPoundsterlingStringWithPerry(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPoundsterlingWithPerryAndBrackets(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	pound_str TEXT;
	pound_name TEXT;
	perry_str TEXT;
	perry_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'фунт стерлингов', 'фунта стерлингов', 'фунтов стерлингов', 'фунта стерлингов', 'фунтов стерлингов', 'фунтов стерлингов', 'фунту стерлингов', 'фунтам стерлингов', 'фунтам стерлингов', '', '', '', padejAr[indexPadeg]);
	
	pound_str = split_part(resultQuery::text, ',', 1);
	pound_name = split_part(resultQuery::text, ',', 2);
	perry_str = split_part(resultQuery::text, ',', 3);
	perry_name = split_part(resultQuery::text, ',', 4);

	perry_name = head.GetPraseEnding(fract_value::int, 'пенс', 'пенса', 'пенсов', 'пенса', 'пенсов', 'пенсов', 'пенсу', 'пенсам', 'пенсам', padejAr[indexPadeg]);
	RETURN concat('(', pound_str, ') ', pound_name, ' ', fract_value, ' ', perry_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetPoundsterlingWithPerryAndBrackets(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetPoundsterlingWithPerryAndBrackets(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetYuanStringWithFyn(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	yuan_str TEXT;
	yuan_name TEXT;
	fyn_str TEXT;
	fyn_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'юань', 'юаня', 'юаней', 'юаня', 'юаней', 'юаней', 'юаню', 'юаням', 'юаням', '', '', '', padejAr[indexPadeg]);
	
	yuan_str = split_part(resultQuery::text, ',', 1);
	yuan_name = split_part(resultQuery::text, ',', 2);
	fyn_str = split_part(resultQuery::text, ',', 3);
	fyn_name = split_part(resultQuery::text, ',', 4);

	fyn_name = head.GetPraseEnding(fract_value::int, 'фынь', 'фыня', 'фыней', 'фыня', 'фыней', 'фыней', 'фыню', 'фыням', 'фыням', padejAr[indexPadeg]);
	RETURN concat(yuan_str, ' ', yuan_name, ' ', fract_value, ' ', fyn_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetYuanStringWithFyn(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetYuanStringWithFyn(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetYuanStringWithFynAndBrackets(value decimal, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	fract_value TEXT = '00';
	padejAr TEXT[6];
	num int;
	resultQuery TEXT;
	yuan_str TEXT;
	yuan_name TEXT;
	fyn_str TEXT;
	fyn_name TEXT;
BEGIN 
	padejAr = enum_range(null::head.padej);
	value = round(value, 2);
	num = TRUNC(value);

	IF POSITION('.' in value::text) > 0 THEN
		fract_value = split_part(value::text, '.', 2);
	END IF;	
	
	IF POSITION(',' in value::text) > 0 THEN
		fract_value = split_part(value::text, ',', 2);
	END IF;			

	SELECT concat(one, ',', two, ',', three, ',', four) INTO STRICT resultQuery FROM head.CurPhrase(num, 'юань', 'юаня', 'юаней', 'юаня', 'юаней', 'юаней', 'юаню', 'юаням', 'юаням', '', '', '', padejAr[indexPadeg]);
	
	yuan_str = split_part(resultQuery::text, ',', 1);
	yuan_name = split_part(resultQuery::text, ',', 2);
	fyn_str = split_part(resultQuery::text, ',', 3);
	fyn_name = split_part(resultQuery::text, ',', 4);

	fyn_name = head.GetPraseEnding(fract_value::int, 'фынь', 'фыня', 'фыней', 'фыня', 'фыней', 'фыней', 'фыню', 'фыням', 'фыням', padejAr[indexPadeg]);
	RETURN concat('(', yuan_str, ') ', yuan_name, ' ', fract_value, ' ', fyn_name);
END
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION head.GetYuanStringWithFynAndBrackets(value text, indexPadeg int)
RETURNS TEXT AS $$
DECLARE 
	valueD decimal;
BEGIN 
	IF value = NULL OR value = '' THEN 
		RETURN NULL;
	END IF; 

	SELECT head.myDecimalTryParce(value) INTO valueD;

	IF valueD = -1 THEN  
		RETURN concat('ВНИМАНИЕ: Не удалось преобразовать значение ', value, ' к десятичному значению!');
	END IF;

	RETURN head.GetYuanStringWithFynAndBrackets(valueD, indexPadeg);
END
$$ LANGUAGE plpgsql;