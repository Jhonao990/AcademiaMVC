-- Execute depois de aplicar as migrations e somente se ainda não houver modalidades.
IF NOT EXISTS (SELECT 1 FROM dbo.Modalidades)
BEGIN
    INSERT INTO dbo.Modalidades
        (Nome, Descricao, Horario, LimiteVagas, PrecoMensal, ImagemUrl, Ativa)
    VALUES
        (N'Musculação', N'Treinamento com pesos e aparelhos.', N'Segunda a sexta, 06h às 22h', 30, 89.90, N'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=900', 1),
        (N'Treinamento Funcional', N'Exercícios dinâmicos para força, equilíbrio e condicionamento.', N'Terças e quintas, 19h', 15, 109.90, N'https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=900', 1),
        (N'Pilates', N'Atividades de mobilidade, postura e controle corporal.', N'Segundas e quartas, 18h', 12, 119.90, N'https://images.unsplash.com/photo-1518611012118-696072aa579a?w=900', 1);
END;
