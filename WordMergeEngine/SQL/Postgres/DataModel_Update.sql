ALTER TABLE public."Paragraph"
ADD COLUMN IF NOT EXISTS "IsCustomNumbering" boolean NOT NULL DEFAULT false;

ALTER TABLE public."Paragraph"
ADD COLUMN IF NOT EXISTS "StartNumberingFrom" character varying(1000) COLLATE pg_catalog."default";

ALTER TABLE public."Paragraph"
ADD COLUMN IF NOT EXISTS "CustomNo" character varying(1000) COLLATE pg_catalog."default";

ALTER TABLE public."Paragraph"
ADD COLUMN IF NOT EXISTS "CustomLevel" integer NOT NULL DEFAULT 0;