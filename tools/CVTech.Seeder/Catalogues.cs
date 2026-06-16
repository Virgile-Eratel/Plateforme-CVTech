namespace CVTech.Seeder;

/// <summary>
/// Vocabulaire métier curé à la main pour donner un rendu crédible « plateforme tech »
/// (intitulés de postes, stacks, domaines réels). Bogus fournit en complément les personnes,
/// entreprises et textes ; ces listes apportent la cohérence sectorielle.
/// </summary>
internal static class Catalogues
{
    public static readonly string[] Domaines =
    [
        "Développement Web",
        "Développement Mobile",
        "Data Engineering",
        "Data Science & IA",
        "DevOps & Cloud",
        "Cybersécurité",
        "Design UX/UI",
        "Product Management",
        "QA & Test",
        "Architecture Logicielle",
    ];

    public static readonly string[] Postes =
    [
        "Développeur Full Stack",
        "Développeur Backend .NET",
        "Développeur Frontend React",
        "Ingénieur Data",
        "Data Scientist",
        "Ingénieur DevOps",
        "Architecte Cloud",
        "Analyste Cybersécurité",
        "Développeur Mobile",
        "Lead Tech",
        "Product Owner",
        "Ingénieur QA",
        "Site Reliability Engineer",
        "Designer UX/UI",
    ];

    public static readonly string[] Seniorites = ["Junior", "Confirmé", "Senior", "Lead"];

    public static readonly string[] Competences =
    [
        "C#", ".NET", "ASP.NET Core", "Blazor", "Entity Framework Core", "React", "Angular",
        "TypeScript", "JavaScript", "Python", "Go", "SQL", "PostgreSQL", "Azure", "AWS",
        "Docker", "Kubernetes", "Terraform", "CI/CD", "Git", "REST", "GraphQL", "gRPC",
        "Microservices", "DDD", "TDD", "RabbitMQ", "Redis", "Elasticsearch", "Power BI",
    ];

    public static readonly string[] Methodologies =
    [
        "Agile Scrum (sprints de 2 semaines)",
        "Kanban en flux continu",
        "SAFe à l'échelle",
        "Extreme Programming (pair programming, TDD)",
    ];

    // Articles éditoriaux : (catégorie 0..3 alignée sur CategorieEditoriale, titre)
    public static readonly (int Categorie, string Titre)[] Articles =
    [
        (0, "Les tendances du recrutement tech en 2026"),
        (0, "Pénurie de profils data : comment les entreprises s'adaptent"),
        (0, "Le retour du présentiel partiel dans les équipes produit"),
        (1, "Grille des salaires développeurs : les chiffres 2026"),
        (1, "TJM freelance : ce qui a changé cette année"),
        (1, "Combien gagne un ingénieur DevOps en France ?"),
        (2, "Blazor United : le pari full-stack .NET tient-il ses promesses ?"),
        (2, "React, Angular, Vue : l'état de l'écosystème frontend"),
        (2, "L'IA générative s'invite dans l'IDE des développeurs"),
        (2, "Rust gagne du terrain côté backend"),
        (3, "Retour d'expérience : migrer un monolithe vers le cloud"),
        (3, "Comment nous avons divisé nos coûts Azure par deux"),
        (3, "Mettre en place une culture TDD dans une équipe legacy"),
    ];

    public static readonly (string Nom, string Url)[] Sources =
    [
        ("LinkedIn Talent Blog", "https://www.linkedin.com/talent/blog"),
        ("Stack Overflow Developer Survey", "https://survey.stackoverflow.co"),
        ("JetBrains State of Developer Ecosystem", "https://www.jetbrains.com/lp/devecosystem"),
        ("GitHub Octoverse", "https://octoverse.github.com"),
    ];
}
