using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Lemko
{
    // The game's texts are written in English in the scenes and in code. When the language is Polish every text is
    // translated on its way to the screen (see LangApply). Lemko words are never touched: only whole English lines,
    // or lines that start with a known English phrase, are replaced.
    public static class Lang
    {
        public static bool Polish;
        public static string T(string english) => Polish ? Translate(english) : english;

        static readonly Dictionary<string,string> Exact=new Dictionary<string,string>
        {
            {"Start again","Zacznij od nowa"},{"Return to village","Wróć do wioski"},
            // places
            {"Shop","Sklep"},{"Farm","Gospodarstwo"},{"House","Dom"},{"Memory","Memory"},{"Numbers","Liczby"},{"Alphabet","Alfabet"},{"Weather","Pogoda"},
            {"Family Tree","Drzewo rodziny"},{"FamilyTree","Drzewo rodziny"},{"Family tree","Drzewo rodziny"},{"Village","Wioska"},
            // buttons and menu
            {"Menu","Menu"},{"Help","Pomoc"},{"Close","Zamknij"},{"Resume","Wróć do gry"},{"Play","Graj"},{"Play again","Zagraj jeszcze raz"},{"Shop again","Jeszcze raz do sklepu"},
            {"Next","Dalej"},{"Previous","Wstecz"},{"Next place","Następne miejsce"},{"View results","Zobacz wyniki"},{"Select","Wybierz"},{"Check","Sprawdź"},{"Check answer","Sprawdź odpowiedź"},{"Check basket","Sprawdź koszyk"},
            {"Listen","Posłuchaj"},{"Speak","Posłuchaj"},{"Pronunciation","Wymowa"},{"Hint","Podpowiedź"},{"Hold for help","Przytrzymaj, aby zobaczyć podpowiedź"},{"Help: one letter","Pomoc: jedna litera"},
            {"Audio","Dźwięk"},{"Audio: on","Dźwięk: włączony"},{"Audio: off","Dźwięk: wyłączony"},{"Motion","Animacje"},{"Motion: reduced","Animacje: ograniczone"},{"Motion: standard","Animacje: zwykłe"},
            {"Hint language","Język"},{"Language: English","Language: English"},{"Language: Polish","Język: polski"},{"Session settings","Ustawienia"},
            {"Show next time: on","Pokazuj następnym razem: tak"},{"Show next time: off","Pokazuj następnym razem: nie"},
            {"Start challenge","Zacznij wyzwanie"},{"Switch to learning","Przejdź do nauki"},{"Fresh attempt","Nowa próba"},{"Unlimited practice","Ćwicz bez limitu"},
            {"Restart with full hearts","Zacznij od nowa z pełnymi sercami"},{"Keep going in Learning mode","Graj dalej w trybie nauki"},{"Keep playing","Graj dalej"},{"New game","Nowa gra"},
            {"Word journal","Dziennik słów"},{"Word","Słowo"},{"Show all encountered","Pokaż wszystkie poznane"},{"Show words to practise","Pokaż słowa do powtórki"},
            {"Search Lemko, Polish or English","Szukaj po łemkowsku, polsku lub angielsku"},{"Lives","Życia"},{"< Room","< Pokój"},{"Room >","Pokój >"},
            {"Gold cloth","Złoty obrus"},{"Red cloth","Czerwony obrus"},{"Warm gold","Ciepłe złoto"},{"River blue","Rzeczny błękit"},
            // title, village, story
            {"Lemko Village","Łemkowska Wioska"},{"LEMKO VILLAGE","ŁEMKOWSKA WIOSKA"},{"A GATHERING FOR THE VILLAGE","SPOTKANIE W WIOSCE"},{"A gathering for the village","Spotkanie w wiosce"},
            {"Explore eight places to learn Lemko","Odwiedź osiem miejsc i ucz się łemkowskiego"},{"A small place to learn - development milestone 1","Małe miejsce do nauki"},
            {"Click or tap anywhere to play","Kliknij lub dotknij, aby zagrać"},{"Your next adventure","Twoja następna przygoda"},
            {"Learn Lemko words while the village gets ready for its gathering.","Ucz się łemkowskich słów, a wioska przygotuje się do spotkania."},
            {"Learning challenge","Wyzwanie językowe"},{"Relaxed experience","Spokojna nauka"},
            {"8 places · limited hearts","8 miejsc · ograniczona liczba serc"},{"8 places · unlimited tries","8 miejsc · dowolna liczba prób"},
            {"Our neighbours are preparing a gathering. Help at any of the eight places.","Sąsiedzi przygotowują spotkanie. Pomóż w dowolnym z ośmiu miejsc."},
            {"The gathering is ready. Choose a keepsake colour, or revisit any place.","Spotkanie jest gotowe. Wybierz kolor na pamiątkę albo odwiedź dowolne miejsce."},
            {"The gathering is ready!","Spotkanie jest gotowe!"},{"Your village gathering is ready!","Spotkanie w wiosce jest gotowe!"},{"Choose a colour below.","Wybierz kolor poniżej."},
            {"Festival time! Replay any place and beat your best.","Czas na święto! Zagraj w dowolnym miejscu i pobij swój wynik."},
            {"Help here","Pomóż tutaj"},{"Ready - practise again","Gotowe - poćwicz jeszcze"},{"Help prepare the gathering.","Pomóż przygotować spotkanie."},
            {"Learning: mistakes never block play.","Nauka: błędy nigdy nie przerywają gry."},{"Village learning","Nauka w wiosce"},
            {"Gather supplies","Zbierz zapasy"},{"Collect supplies for the shared table.","Zbierz zapasy na wspólny stół."},{"The supplies are ready.","Zapasy są gotowe."},
            {"Deliver invitations","Roznieś zaproszenia"},{"Invite the relatives shown in the family tree.","Zaproś krewnych z drzewa rodziny."},{"The invitations are ready.","Zaproszenia są gotowe."},
            {"Prepare the animals","Przygotuj zwierzęta"},{"Help finish the animal pens before the gathering.","Pomóż dokończyć zagrody przed spotkaniem."},{"The animal pens are ready.","Zagrody są gotowe."},
            {"Prepare the room","Przygotuj pokój"},{"Prepare furniture in the requested room.","Ustaw meble we wskazanym pokoju."},{"The room is ready.","Pokój jest gotowy."},
            {"Pack the checklist","Spakuj rzeczy z listy"},{"Match the packing checklist pictures and words.","Dopasuj obrazki z listy do słów."},{"The packing checklist is ready.","Lista rzeczy jest gotowa."},
            {"Prepare numbers","Przygotuj numery"},{"Choose the requested numbers.","Wybierz właściwe numery."},{"The numbers are ready.","Numery są gotowe."},
            {"Prepare the labels","Przygotuj etykiety"},{"Check the spelling for our preparations.","Sprawdź pisownię na etykietach."},{"The spelling preparations are ready.","Etykiety są gotowe."},
            {"Plan the weather","Zaplanuj pogodę"},{"Set the requested weather and temperature for our planned day.","Ustaw pogodę i temperaturę na zaplanowany dzień."},{"The weather plan is ready.","Plan pogody jest gotowy."},
            {"Contribution ready","Gotowe"},{"Contribution complete.","Gotowe."},{"Practice complete","Ćwiczenie skończone"},
            {"Your contribution is already ready. Thank you for practising.","To już było gotowe. Dziękujemy za ćwiczenie."},
            {"Your gathering preparations are ready. Revisit any place whenever you like.","Przygotowania są gotowe. Wracaj do dowolnego miejsca, kiedy chcesz."},
            {"Choose another activity whenever you are ready.","Wybierz kolejne miejsce, kiedy zechcesz."},
            // introductions
            {"Right-click or press and hold a word or picture to see its meaning.","Kliknij słowo lub obrazek prawym przyciskiem albo przytrzymaj palec, aby zobaczyć znaczenie."},
            {"Tap the tray to peek at one card.","Dotknij tacki, aby podejrzeć jedną kartę."},
            {"Finish the labels","Dokończ etykiety"},{"Some labels still need to be written or repaired. Help spell the words in Lemko.","Niektóre etykiety trzeba jeszcze napisać lub poprawić. Pomóż zapisać słowa po łemkowsku."},
            {"1. Look at the picture and its meaning. Right-click either for the Lemko spelling.","1. Spójrz na obrazek i jego znaczenie. Kliknij prawym przyciskiem, aby zobaczyć łemkowską pisownię."},
            {"2. Click or drag letters into the slots. Tap a filled slot to return its letter.","2. Klikaj lub przeciągaj litery na pola. Dotknij zajętego pola, aby oddać literę."},
            {"3. Finish or repair the word, then choose Check answer.","3. Dokończ lub popraw słowo, a potem wybierz Sprawdź."},
            {"Invite the family","Zaproś rodzinę"},{"A gathering feels better with family around. Work out who is being invited, using the person marked 'you' as your reference.","Spotkanie jest lepsze z rodziną. Ustal, kogo zaprosić, zaczynając od osoby oznaczonej jako TY."},
            {"1. Find the person marked 'you'.","1. Znajdź osobę oznaczoną jako TY."},{"2. Read the requested relative in Lemko.","2. Przeczytaj po łemkowsku, kogo szukasz."},
            {"3. Select a matching person. Some questions accept more than one person.","3. Wybierz pasującą osobę. Czasem pasuje więcej niż jedna."},
            {"Look after the animals","Zajmij się zwierzętami"},{"Before the neighbours meet, the farm animals need your attention. Help prepare their pens.","Zanim sąsiedzi się spotkają, trzeba zająć się zwierzętami. Pomóż przygotować zagrody."},
            {"1. Read the requested animal and quantity.","1. Przeczytaj, jakie zwierzę i ile sztuk jest potrzebne."},
            {"2. Drag animals into the pen, or tap to add them. Tap placed animals to remove them.","2. Przeciągnij zwierzęta do zagrody albo dotknij, aby je dodać. Dotknij zwierzęcia w zagrodzie, aby je zabrać."},
            {"3. Repair the pen if needed, then choose Check answer.","3. Popraw zagrodę, jeśli trzeba, a potem wybierz Sprawdź."},
            {"1. Read the animal name.","1. Przeczytaj nazwę zwierzęcia."},{"2. Drag or tap the matching animal into the pen. Another choice replaces it.","2. Przeciągnij lub dotknij właściwe zwierzę. Kolejny wybór je zastąpi."},
            {"3. Choose Check answer. Quantities become available after a Numbers session.","3. Wybierz Sprawdź. Ilości pojawią się po grze w Liczby."},
            {"Make the house welcoming","Urządź dom"},{"Our neighbours are making space for the gathering. Put the requested furniture in the right room.","Sąsiedzi robią miejsce na spotkanie. Postaw wskazany mebel we właściwym pokoju."},
            {"1. Read which piece goes in which room. The arrows at the sides of the screen take you through the house.","1. Przeczytaj, co ma stanąć w którym pokoju. Strzałki po bokach ekranu prowadzą przez dom."},
            {"2. Find the piece in the bar at the bottom and drag it into the right room.","2. Znajdź przedmiot na pasku u dołu i przeciągnij go do właściwego pokoju."},
            {"3. Pieces you have placed stay there. Drag one to move it.","3. Postawione przedmioty zostają na miejscu. Przeciągnij, aby przesunąć."},
            {"Pack for the gathering","Spakuj się na spotkanie"},{"We have pictures and Lemko words for the supplies. Pair them up to help everyone pack.","Mamy obrazki i łemkowskie słowa. Połącz je w pary, żeby wszyscy mogli się spakować."},
            {"1. Turn over two cards: match a picture with its Lemko word.","1. Odkryj dwie karty: dopasuj obrazek do łemkowskiego słowa."},
            {"2. Keep matching until the board is cleared. Mismatches do not cost lives.","2. Dopasowuj, aż znikną wszystkie karty. Pomyłki nie zabierają żyć."},
            {"3. Finish the short picture-to-word checks afterwards.","3. Na koniec rozwiąż krótkie zadania: obrazek i słowo."},
            {"Recognise the numbers","Rozpoznaj numery"},{"Read or listen to each request and choose the right number.","Przeczytaj lub posłuchaj i wybierz właściwy numer."},
            {"1. Read the Lemko number, or play its recording when available.","1. Przeczytaj łemkowską liczbę albo posłuchaj nagrania."},{"2. Select the matching number.","2. Wybierz pasujący numer."},
            {"3. Finish the session to unlock animal quantities at the Farm.","3. Ukończ grę, aby odblokować ilości zwierząt w Gospodarstwie."},
            {"Gather the supplies","Zbierz zapasy"},{"The village is preparing a shared meal. Help collect everything on the shopping list.","Wioska szykuje wspólny posiłek. Pomóż zebrać wszystko z listy zakupów."},
            {"1. Read the Lemko shopping list.","1. Przeczytaj łemkowską listę zakupów."},{"2. Click or tap products to add them to your basket. Tap again to remove.","2. Kliknij produkt z listy, a trafi do koszyka."},
            {"3. Choose Check basket when everything is ready.","3. Gra kończy się sama, gdy masz wszystko z listy."},
            {"Plan a day together","Zaplanuj wspólny dzień"},{"Our neighbours have a day out in mind. Match the weather and temperature to each request.","Sąsiedzi planują wycieczkę. Dopasuj pogodę i temperaturę do każdej prośby."},
            {"1. Read the weather request.","1. Przeczytaj, jaka ma być pogoda."},{"2. Select the weather and move the temperature slider.","2. Wybierz pogodę i ustaw suwak temperatury."},
            {"3. Choose Check answer. Adjust your choices until the outing is ready.","3. Wybierz Sprawdź. Poprawiaj, aż wszystko będzie się zgadzać."},
            // tasks and feedback
            {"Choose the products on your list.","Wybierz produkty z listy."},{"Find everything on the list.","Znajdź wszystko z listy."},{"That is not on the list.","Tego nie ma na liście."},{"One more thing.","Jeszcze jedna rzecz."},
            {"Everything on the list!","Masz wszystko z listy!"},{"SHOPPING LIST","LISTA ZAKUPÓW"},
            {"Match pictures and words","Dopasuj obrazki do słów"},{"Turn over two cards.","Odkryj dwie karty."},{"Not a pair. Try again.","To nie para. Spróbuj jeszcze raz."},{"All pairs found.","Wszystkie pary znalezione."},
            {"All matched and checked!","Wszystko dopasowane i sprawdzone!"},{"That word belongs to another picture.","To słowo pasuje do innego obrazka."},
            {"Start from YOU and follow the lines.","Zacznij od TY i idź po liniach."},{"Invitation delivered!","Zaproszenie doręczone!"},{"Not this relative. Follow the lines from YOU.","To nie ten krewny. Idź po liniach od TY."},{"YOU","TY"},
            {"Put the animal in the pasture.","Postaw zwierzę na pastwisku."},{"Something is wrong in this pen.","W tej zagrodzie coś się nie zgadza."},{"Drag or tap animals into the pen. Tap one to remove it.","Przeciągnij lub dotknij zwierzęta, aby trafiły do zagrody. Dotknij, aby zabrać."},
            {"That is a different animal.","To inne zwierzę."},{"Just right!","W sam raz!"},{"Right piece, but this is another room.","Dobry przedmiot, ale to inny pokój."},{"That is not the piece we need.","To nie ten przedmiot."},
            {"The piece is still waiting in the bar.","Przedmiot wciąż czeka na pasku."},{"That is a different number.","To inny numer."},
            {"Tap or drag letters. Tap a slot to undo.","Dotykaj lub przeciągaj litery. Dotknij pola, aby cofnąć."},{"One letter added for you.","Dodano jedną literę."},{"Not quite. Check each letter.","Jeszcze nie. Sprawdź każdą literę."},
            {"Pick a weather picture.","Wybierz obrazek pogody."},{"That is different weather.","To inna pogoda."},{"Too cold.","Za zimno."},{"Too warm.","Za ciepło."},{"A perfect day for it!","Idealny dzień!"},
            {"On a roll!","Świetna passa!"},{"Good try!","Dobra próba!"},{"Practise next:","Do powtórki:"},
            {"Out of hearts!\nTake a breath, then carry on.","Koniec serc!\nOdetchnij i graj dalej."},{"No lives left","Koniec żyć"},{"Challenge lives used up.","Życia w wyzwaniu się skończyły."},
            {"Session complete!","Gra ukończona!"},{"Shopping complete!","Zakupy zrobione!"},{"Attempt finished","Koniec próby"},{"Learning session","Tryb nauki"},{"Learning","Nauka"},{"Challenge","Wyzwanie"},
            {"These checks are supported practice, not independent recall.","Te zadania to ćwiczenie z pomocą, a nie samodzielne przypominanie."},
            {"Animal quantity tasks are now available at Farm!","W Gospodarstwie są teraz zadania z liczeniem zwierząt!"},
            // journal
            {"Encountered","Poznane"},{"Recalled on a later day","Przypomniane innego dnia"},{"Answered without help","Odpowiedź bez pomocy"},{"Practising with support","Ćwiczone z pomocą"},
            {"No words here yet. Play an activity to encounter vocabulary.","Nie ma tu jeszcze słów. Zagraj, aby je poznać."},{"No matching encountered words. Try another search.","Brak pasujących słów. Spróbuj wyszukać inaczej."},
            {"Encountered; no answer history yet.","Poznane; jeszcze bez odpowiedzi."},{"Language content awaiting review.","Treść językowa czeka na sprawdzenie."},{"Missing word","Brak słowa"},
            {"stars","gwiazdki"},{"word met","poznane słowo"},{"words met","poznane słowa"},{"best streak","najlepsza passa"},{"minute","minuta"},{"minutes","minuty"},{"Content needs attention","Treść wymaga poprawy"},
        };

        // Lines with changing parts. {0}, {1} stand for a number, a Lemko word or a place name.
        static readonly string[,] Shapes=
        {
            {"{0} more things.","Jeszcze {0}."},{"Pair found! {0}/{1}","Para! {0}/{1}"},{"All pairs found! Now {0} quick word checks.","Wszystkie pary znalezione! Teraz {0} krótkie zadania ze słowami."},
            {"Word check {0}/{1}: which word is this?","Zadanie {0}/{1}: które to słowo?"},{"Pick the weather and set {0} to {1} °C.","Wybierz pogodę i ustaw od {0} do {1} °C."},
            {"Number {0} is ready!","Numer {0} jest gotowy!"},{"{0} complete!","{0}: ukończono!"},{"{0} points","{0} pkt"},{"{0} correct","dobrze: {0}"},{"{0} mistake","błędy: {0}"},{"{0} mistakes","błędy: {0}"},
            {"{0} hint","podpowiedzi: {0}"},{"{0} hints","podpowiedzi: {0}"},{"{0} hearts left","zostało serc: {0}"},{"best this session {0}","najlepszy wynik {0}"},
            {"Journal · {0} word","Dziennik · {0} słowo"},{"Journal · {0} words","Dziennik · słowa: {0}"},{"Word journal - {0} encountered words","Dziennik słów - poznane słowa: {0}"},
            {"Gathering preparations: {0} / 8","Przygotowania do spotkania: {0} / 8"},{"Invited: {0} / {1}","Zaproszeni: {0} / {1}"},{"Challenge: {0} shared lives. Hints and rewards stay available.","Wyzwanie: {0} wspólnych żyć. Podpowiedzi i nagrody zostają."},
            {"{0} — {1}","{0} — {1}"},{"{0} ({1}/8)","{0} ({1}/8)"},{"(+{0} more in the journal)","(+{0} w dzienniku)"},
        };
        // Phrases that open a line and are followed by something that stays as it is (usually a Lemko word).
        static readonly string[,] Openers=
        {
            {"Deliver an invitation to ","Zanieś zaproszenie: "},{"Fix the label: ","Popraw etykietę: "},{"Spell in Lemko: ","Zapisz po łemkowsku: "},{"Fix the pen: ","Popraw zagrodę: "},{"Fill the pen: ","Zapełnij zagrodę: "},
            {"Find the number ","Znajdź numer "},{"Find ","Znajdź: "},{"Place ","Postaw: "},{"Label ready: ","Etykieta gotowa: "},{"Yes: ","Tak: "},{"Try another: ","Spróbuj inne: "},{"Next: ","Dalej: "},
            {"Village gathering: ","Spotkanie w wiosce: "},{"Practise next: ","Do powtórki: "},{"Words practised: ","Ćwiczone słowa: "},{"Add the missing letters.","Dodaj brakujące litery."},
            {"That is not on the list. Look for: ","Tego nie ma na liście. Szukaj: "},{"Points: ","Punkty: "},{"Best: ","Rekord: "},{"Correct: ","Dobrze: "},{"Mistakes: ","Błędy: "},{"Hints used: ","Podpowiedzi: "},
            {"Challenge lives remaining: ","Zostało żyć: "},{"Independent: ","Samodzielnie: "},{"Supported: ","Z pomocą: "},{"Words repaired: ","Poprawione słowa: "},{"Pairs: ","Pary: "},{"Association checks: ","Zadania ze słowami: "},
            {"Check mistakes: ","Błędy w zadaniach: "},{"Without help: ","Bez pomocy: "},{"With support: ","Z pomocą: "},{"Recording not supplied","Brak nagrania"},{"Shop completed: ","Sklep ukończony: "},{"Shopping list: ","Lista zakupów: "},
        };
        // Phrases that may be added to the end of a line.
        static readonly string[,] Closers=
        {
            {" Right-click or press and hold a word for help."," Kliknij słowo prawym przyciskiem albo przytrzymaj palec, aby zobaczyć podpowiedź."},
        };
        static readonly string[] Dividers={ "\n","  •  ","   ·   ","  |  "," | " };
        static readonly Regex Bonus=new Regex(@"^(.*?)  Bonus \+(\d+)!$",RegexOptions.Singleline);
        static List<KeyValuePair<Regex,string>> shapes;
        static readonly Dictionary<string,string> cache=new Dictionary<string,string>();

        public static string Translate(string text)
        {
            if(string.IsNullOrEmpty(text)) return text;
            if(cache.TryGetValue(text,out var known)) return known;
            string result=Work(text,0);
            if(cache.Count>4000) cache.Clear();
            cache[text]=result; return result;
        }
        static bool Latin(string s) { foreach(char c in s) if((c>='a' && c<='z') || (c>='A' && c<='Z')) return true; return false; }
        static string Work(string text,int depth)
        {
            if(depth>6 || !Latin(text)) return text;
            if(Exact.TryGetValue(text,out var exact)) return exact;
            string trimmed=text.Trim();
            if(trimmed.Length!=text.Length && trimmed.Length>0) { int at=text.IndexOf(trimmed,System.StringComparison.Ordinal); return text.Substring(0,at)+Work(trimmed,depth+1)+text.Substring(at+trimmed.Length); }
            foreach(string divider in Dividers) if(text.Contains(divider)) {
                var parts=text.Split(new[]{divider},System.StringSplitOptions.None);
                for(int i=0;i<parts.Length;i++) parts[i]=Work(parts[i],depth+1);
                return string.Join(divider,parts);
            }
            var bonus=Bonus.Match(text); if(bonus.Success) return Work(bonus.Groups[1].Value,depth+1)+"  Premia +"+bonus.Groups[2].Value+"!";
            for(int i=0;i<Closers.GetLength(0);i++) if(text.EndsWith(Closers[i,0],System.StringComparison.Ordinal) && text.Length>Closers[i,0].Length)
                return Work(text.Substring(0,text.Length-Closers[i,0].Length),depth+1)+Closers[i,1];
            if(shapes==null) {
                shapes=new List<KeyValuePair<Regex,string>>();
                for(int i=0;i<Shapes.GetLength(0);i++) {
                    string pattern="^"+Regex.Escape(Shapes[i,0]).Replace(@"\{0}","(.+?)").Replace(@"\{1}","(.+?)")+"$";
                    shapes.Add(new KeyValuePair<Regex,string>(new Regex(pattern,RegexOptions.Singleline),Shapes[i,1]));
                }
            }
            foreach(var shape in shapes) {
                var match=shape.Key.Match(text); if(!match.Success) continue;
                string a=match.Groups.Count>1 ? Work(match.Groups[1].Value,depth+1) : "", b=match.Groups.Count>2 ? Work(match.Groups[2].Value,depth+1) : "";
                return shape.Value.Replace("{0}",a).Replace("{1}",b);
            }
            // The longest opening phrase wins, and whatever follows it is looked at again.
            int best=-1; for(int i=0;i<Openers.GetLength(0);i++) if(text.StartsWith(Openers[i,0],System.StringComparison.Ordinal) && (best<0 || Openers[i,0].Length>Openers[best,0].Length)) best=i;
            if(best>=0) return Openers[best,1]+Work(text.Substring(Openers[best,0].Length),depth+1);
            // A whole sentence followed by more text, such as an instruction and then a hint.
            int stop=text.IndexOf(". ",System.StringComparison.Ordinal);
            if(stop>0 && Exact.TryGetValue(text.Substring(0,stop+1),out var first)) return first+" "+Work(text.Substring(stop+2),depth+1);
            return text;
        }
    }
}
