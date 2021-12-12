
CREATE function Ep.[fCharCount]( @string varchar(128), @c char ) returns int
as
begin
	 return len(@string) - len(replace(@string, @c, ''))
end
