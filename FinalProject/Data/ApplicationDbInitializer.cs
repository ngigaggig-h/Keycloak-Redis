using FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Data;

// Ініціалізує схему та стартові дані для локальної БД.
public static class ApplicationDbInitializer
{
    private const string SeedOrganizerId = "seed-organizer";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        await SeedCategoriesAsync(dbContext);
        await SeedOrganizerAsync(dbContext);
        await SeedEventsAsync(dbContext);
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext dbContext)
    {
        var categories = new[]
        {
            new Category { Name = "Technology", Slug = "technology" },
            new Category { Name = "Business", Slug = "business" },
            new Category { Name = "Design", Slug = "design" },
            new Category { Name = "Marketing", Slug = "marketing" },
            new Category { Name = "Startup", Slug = "startup" },
            new Category { Name = "Career", Slug = "career" }
        };

        foreach (var category in categories)
        {
            if (!await dbContext.Categories.AnyAsync(existingCategory => existingCategory.Slug == category.Slug))
            {
                dbContext.Categories.Add(category);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedOrganizerAsync(ApplicationDbContext dbContext)
    {
        var organizer = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == SeedOrganizerId);
        if (organizer != null)
        {
            return;
        }

        dbContext.Users.Add(new User
        {
            Id = SeedOrganizerId,
            Name = "PodiiHub Team",
            Email = "events@podiihub.local",
            PasswordHash = "seed-password-hash",
            Salt = "seed-salt",
            Role = "Organizer"
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedEventsAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Events.AnyAsync())
        {
            return;
        }

        var categoryIds = await dbContext.Categories
            .ToDictionaryAsync(category => category.Slug, category => category.Id);

        var baseDate = new DateTime(2026, 7, 3, 9, 0, 0, DateTimeKind.Utc);

        var events = new[]
        {
            new Event
            {
                Title = "Kyiv AI Product Forum",
                TitleDescription = "Практичний форум про AI-фічі, roadmap та реальні кейси запуску.",
                Description = "Одноденний форум для product managers, founders і team leads. У програмі: як інтегрувати AI у продукт без псевдо-інновацій, як рахувати ROI автоматизації, як запускати copilots і internal tooling, не ламаючи UX та процеси команди.",
                Location = "UNIT.City, Kyiv",
                StartAt = baseDate.AddDays(2).AddHours(1),
                Capacity = 180,
                ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["technology"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Lviv Design Systems Meetup",
                TitleDescription = "Вечірня зустріч для дизайнерів і фронтенд-команд про масштабування UI.",
                Description = "Мітап про бібліотеки компонентів, токени дизайну, handoff без болю та узгодження між продуктовими й маркетинговими командами. Частина доповідей присвячена доступності, адаптивності і підтримці великих дизайн-систем у B2B-продуктах.",
                Location = "FESTrepublic, Lviv",
                StartAt = baseDate.AddDays(4).AddHours(8),
                Capacity = 120,
                ImageUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["design"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Dnipro Startup Breakfast",
                TitleDescription = "Неформальна зустріч засновників про продажі, funding і перший traction.",
                Description = "Камерний сніданок для early-stage startup teams. Поговоримо про customer discovery, підготовку до investor updates, продажі в B2B SaaS і те, як не витратити пів року на фічі, які ніхто не купить.",
                Location = "Menorah Center, Dnipro",
                StartAt = baseDate.AddDays(6).AddHours(0),
                Capacity = 70,
                ImageUrl = "https://images.unsplash.com/photo-1515169067868-5387ec356754?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["startup"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Odesa Growth Marketing Day",
                TitleDescription = "Конференція про performance, retention і контент-воронки для digital-команд.",
                Description = "Повноформатний захід для marketers, growth managers і founders. У фокусі: аналітика каналів, бренд plus performance, email automation, органічний контент і системне тестування гіпотез без зливу бюджету.",
                Location = "Impact Hub Odesa, Odesa",
                StartAt = baseDate.AddDays(8).AddHours(2),
                Capacity = 160,
                ImageUrl = "https://images.unsplash.com/photo-1505373877841-8d25f7d46678?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["marketing"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Kharkiv Engineering Leaders Circle",
                TitleDescription = "Закрите коло для engineering managers про delivery та архітектурні рішення.",
                Description = "Серія коротких виступів і круглий стіл про найм, технічний борг, як не втрачати швидкість після росту команди та як приймати архітектурні рішення, коли продукт змінюється швидше за документацію.",
                Location = "YermilovCentre, Kharkiv",
                StartAt = baseDate.AddDays(10).AddHours(3),
                Capacity = 90,
                ImageUrl = "https://images.unsplash.com/photo-1522202176988-66273c2fd55f?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["technology"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Kyiv Career Switch Bootcamp",
                TitleDescription = "Інтенсив для тих, хто переходить у tech з інших сфер.",
                Description = "Практичний день з career-консультантами, рекрутерами та hiring managers. Буде розбір CV, LinkedIn, супровідних листів, симуляція інтерв'ю та чесна розмова про стартові ролі у support, QA, marketing та project coordination.",
                Location = "Kooperativ, Kyiv",
                StartAt = baseDate.AddDays(12).AddHours(4),
                Capacity = 140,
                ImageUrl = "https://images.unsplash.com/photo-1521737604893-d14cc237f11d?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["career"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Lviv B2B Sales Lab",
                TitleDescription = "Майстерня про enterprise-продажі, discovery calls і комерційні пропозиції.",
                Description = "Одноденний воркшоп для sales teams та founders, які продають сервіси або SaaS для бізнесу. Розберемо discovery, демонстрації продукту, follow-up, objection handling і побудову pipeline без хаосу в CRM.",
                Location = "Leoland Event Hall, Lviv",
                StartAt = baseDate.AddDays(14).AddHours(5),
                Capacity = 110,
                ImageUrl = "https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["business"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Poltava UX Writing Session",
                TitleDescription = "Практика мікрокопірайтингу, порятунку onboarding і clean UX-текстів.",
                Description = "Короткий інтенсив з вправами для дизайнерів, контент-спеціалістів і PM. Працюємо на реальних екранах: empty states, error messages, forms, pricing pages та сценарії, де текст або рятує продукт, або непомітно його ламає.",
                Location = "Spalah Hub, Poltava",
                StartAt = baseDate.AddDays(16).AddHours(7),
                Capacity = 85,
                ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["design"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Vinnytsia Founder Office Hours",
                TitleDescription = "Серія коротких one-to-one сесій для молодих команд і solo founders.",
                Description = "Формат office hours з менторами з продукту, маркетингу та операцій. Кожна команда отримає зворотний зв'язок по positioning, first paying customers, GTM-плану й структурі найближчих експериментів на 30 днів.",
                Location = "Cherdak Coworking, Vinnytsia",
                StartAt = baseDate.AddDays(18).AddHours(1),
                Capacity = 60,
                ImageUrl = "https://images.unsplash.com/photo-1515187029135-18ee286d815b?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["startup"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Zaporizhzhia Data & Analytics Meetup",
                TitleDescription = "Мітап про product analytics, dashboards і культуру рішень на даних.",
                Description = "Поговоримо про SQL для продуктових команд, побудову зрозумілих dashboard-ів, product metrics, retention cohorts і головне: як не перетворити аналітику на красиві графіки, що не впливають на рішення.",
                Location = "Loft Mlyn, Zaporizhzhia",
                StartAt = baseDate.AddDays(20).AddHours(6),
                Capacity = 95,
                ImageUrl = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["technology"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Chernihiv Product Discovery Workshop",
                TitleDescription = "Воркшоп про інтерв'ю з користувачами, JTBD і пріоритезацію проблем.",
                Description = "Тут не буде абстрактних слайдів. Команди принесуть свої гіпотези й під керівництвом фасилітатора переформулюють їх у дослідницькі питання, customer pains та вимірювані сигнали успіху для наступного спринту.",
                Location = "River Hall, Chernihiv",
                StartAt = baseDate.AddDays(22).AddHours(4),
                Capacity = 100,
                ImageUrl = "https://images.unsplash.com/photo-1517048676732-d65bc937f952?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["business"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Ivano-Frankivsk Content Sprint",
                TitleDescription = "Інтенсив для бренд-команд про серії контенту, медіаплани і редакційний темп.",
                Description = "Розберемо, як будувати контент-систему для продукту чи сервісного бізнесу: від темника і формату рубрик до контент-календаря, repurposing і метрик, які показують не лише охоплення, а й бізнес-ефект.",
                Location = "Promprylad.Renovation, Ivano-Frankivsk",
                StartAt = baseDate.AddDays(24).AddHours(2),
                Capacity = 130,
                ImageUrl = "https://images.unsplash.com/photo-1497366412874-3415097a27e7?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["marketing"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Ternopil Junior Tech Launchpad",
                TitleDescription = "Подія для junior-фахівців з практикою портфоліо, networking та speed-mentoring.",
                Description = "У програмі короткі лекції про старт кар'єри, розбір pet-проєктів, найчастіші помилки в CV і живі speed-mentoring сесії з інженерами, дизайнерами та PM-ами, які реально наймають людей у команди.",
                Location = "Na Poshti Creative Space, Ternopil",
                StartAt = baseDate.AddDays(26).AddHours(3),
                Capacity = 150,
                ImageUrl = "https://images.unsplash.com/photo-1528605248644-14dd04022da1?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["career"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Kyiv SaaS Pricing Intensive",
                TitleDescription = "Глибокий розбір pricing-моделей, пакетів і апсейлу для B2B SaaS.",
                Description = "Практичний інтенсив для founders і revenue-команд. Розберемо price metric, packaging, скидки, onboarding, expansion revenue та сигнали, що твоя цінова модель вже гальмує зростання.",
                Location = "Creative State Arsenal, Kyiv",
                StartAt = baseDate.AddDays(28).AddHours(5),
                Capacity = 125,
                ImageUrl = "https://images.unsplash.com/photo-1559136555-9303baea8ebd?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["business"],
                OrganizerId = SeedOrganizerId
            },
            new Event
            {
                Title = "Lutsk Startup Demo Night",
                TitleDescription = "Вечір коротких пітчів, демо продуктів і зворотного зв'язку від журі.",
                Description = "Фінальний демо-вечір для ранніх продуктових команд. Кожен стартап покаже продукт, current traction і наступну віху росту, а журі дасть конкретний фідбек по позиціонуванню, onboarding, продажах і інвесторській готовності.",
                Location = "Adrenaline City, Lutsk",
                StartAt = baseDate.AddDays(30).AddHours(8),
                Capacity = 170,
                ImageUrl = "https://images.unsplash.com/photo-1475721027785-f74eccf877e2?auto=format&fit=crop&w=1200&q=80",
                CategoryId = categoryIds["startup"],
                OrganizerId = SeedOrganizerId
            }
        };

        dbContext.Events.AddRange(events);
        await dbContext.SaveChangesAsync();
    }
}
