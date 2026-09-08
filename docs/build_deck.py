"""
Build the FaithTech Toronto "AI Build Night" participant deck.

This script is the source of truth; docs/AI-Build-Night.pptx is always
regenerated from it and should never be hand-edited.

    python docs/build_deck.py

The deck is what the room sees on the TV. The operating detail -- leads,
durations, contingencies, the say-lines -- lives in the speaker notes so the
run sheet sits under every slide in Presenter View.
Content is drawn from docs/run-sheet.html and docs/prompt.md.

Layout is measured, not estimated: text is wrapped with the real font files via
Pillow, and PowerPoint's line advance (size x spacing x 1.2, verified against a
rendered slide) is applied so every block reports an exact bottom edge. Slides
flow from a cursor and a build-time guard fails the run if anything crosses
CONTENT_BOTTOM.
"""

import os
import sys

from PIL import ImageFont
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Inches, Pt

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "AI-Build-Night.pptx")
LOGO = os.path.join(HERE, "assets", "faithtech-logo.png")
FONT_DIR = os.path.join(os.environ.get("WINDIR", r"C:\Windows"), "Fonts")

# --------------------------------------------------------------------------
# Brand tokens, read off the live faithtech.com Webflow CSS.
# --------------------------------------------------------------------------
INK = RGBColor(0x16, 0x16, 0x0C)        # --swatch--dark
PAPER = RGBColor(0xE4, 0xE0, 0xD8)      # --brand--grey-300
CARD = RGBColor(0xF5, 0xF0, 0xF0)       # --brand--grey-100
LIME = RGBColor(0xC6, 0xFB, 0x50)       # --swatch--brand
BLUE = RGBColor(0x1D, 0x8F, 0xB9)       # --brand--blue-200
GREEN = RGBColor(0x32, 0xA4, 0x32)      # --brand--green-200
ORANGE = RGBColor(0xF0, 0x52, 0x28)     # --brand--orange-200
AMBER = RGBColor(0xFF, 0xB3, 0x00)      # --brand--orange-100
RULE = RGBColor(0xC6, 0xC5, 0xBB)       # --brand--grey-500
MUTED = RGBColor(0x5E, 0x5C, 0x53)      # ink-soft; 5.2:1 on paper, 6.1:1 on card
WHITE = RGBColor(0xFF, 0xFF, 0xFF)

# Lime and amber are never text on paper -- contrast fails. They appear only as
# a filled block behind INK text, a rule, or a spine.

SANS = "Segoe UI"
SANS_SB = "Segoe UI Semibold"
MONO = "Consolas"
SERIF = "Palatino Linotype"          # FaithTech's own declared Avril fallback

# Type floors -- this is shown on a TV, not a projection wall.
T_TITLE = 44
T_TITLE_MIN = 26
T_BODY = 20
T_CMD_MAX = 28
T_CMD_MIN = 20
T_FOOT = 12
T_POETIC = 14                          # 5R poetic names, inside a 1.9in card

# PowerPoint advances a line by size x line_spacing x 1.2. Measured, not assumed:
# a 20pt paragraph at spacing 1.20 renders a 0.4000in advance = 28.8pt.
PP_LINE = 1.2

# Every wrap decision is made against this fraction of the real box width, so
# the predicted line count is never lower than what PowerPoint renders. These
# boxes are fixed-height with no autofit, so an unforeseen extra line does not
# reflow -- it bleeds over whatever sits below, or outside its card.
#
# Calibrated, not guessed: a deck of strings measured to known fractions of
# their box was rendered through PowerPoint and the lines counted off the
# pixels. Segoe UI / Segoe UI Semibold at 20-34pt held one line at every ratio
# up to 0.9855 and broke at 1.0073, so PowerPoint's own threshold sits at ~1.00
# and Pillow's measurement is accurate rather than optimistic. 0.97 leaves a
# 3% buffer for the presenting laptop having a different build of Segoe UI,
# which costs at most a 1pt step-down on the two widest lines in the deck.
FIT = 0.97

# Breathing room added to every measured box height, so a rounding difference
# can never clip a descender or push text onto the shape below.
SLACK = 0.06

SW, SH = 13.333, 7.5                  # 16:9
MARGIN = 0.85
CW = SW - 2 * MARGIN                  # content width, 11.633"
BODY_TOP = 2.02
CONTENT_BOTTOM = 6.34                 # nothing may cross this
FOOT_RULE = 6.62
FOOT_Y = 6.78

PHASE_COLOR = {
    "GATHER": INK,
    "DISCOVER": BLUE,
    "DISCERN": GREEN,
    "DEVELOP": ORANGE,
    "DEMONSTRATE": AMBER,
    "SEND": INK,
}
PHASE_TEXT = {"DEMONSTRATE": INK}            # amber needs dark text

prs = Presentation()
prs.slide_width = Inches(SW)
prs.slide_height = Inches(SH)
BLANK = prs.slide_layouts[6]

_overflows = []


def guard(y, label):
    """Fail the build if a slide's content crosses into the footer."""
    if y > CONTENT_BOTTOM:
        _overflows.append((len(prs.slides._sldIdLst), label, round(y, 3)))
    return y


def audit():
    """Re-measure the finished deck and report anything that bleeds.

    guard() only sees the blocks whose callers passed a label, and it only ever
    looked downward at the footer. This walks the built file instead: every
    textbox is re-wrapped at its real width, and anything taller than its own
    frame, past CONTENT_BOTTOM, or sitting on top of a neighbour is reported.
    """
    from pptx.util import Emu

    def inches(v):
        return Emu(v).inches

    problems = []
    for n, slide in enumerate(prs.slides, 1):
        boxes = []
        for sh in slide.shapes:
            if not sh.has_text_frame or not sh.text_frame.text.strip():
                continue
            x, y = inches(sh.left), inches(sh.top)
            w, h = inches(sh.width), inches(sh.height)
            need = 0.0
            for p in sh.text_frame.paragraphs:
                text = "".join(r.text for r in p.runs)
                if not text.strip():
                    continue
                r0 = p.runs[0]
                size = r0.font.size.pt if r0.font.size else T_BODY
                lines = len(wrap(text, w, size, r0.font.name or SANS,
                                 r0.font.bold, r0.font.italic))
                before = p.space_before.pt / 72.0 if p.space_before else 0.0
                spacing = p.line_spacing or 1.0
                need += before + lines * size * spacing * PP_LINE / 72.0
            label = sh.text_frame.text.replace("\n", " / ")[:52]
            if need > h + 0.005:
                problems.append((n, "overflows its frame by %.2f in" % (need - h),
                                 label))
            # The footer sits below CONTENT_BOTTOM on purpose.
            if y < FOOT_RULE and y + need > CONTENT_BOTTOM + 0.01:
                problems.append((n, "crosses CONTENT_BOTTOM (%.2f)" % (y + need),
                                 label))
            if y < FOOT_RULE:
                boxes.append((y, y + need, x, x + w, label))
        for i, a in enumerate(boxes):
            for b in boxes[i + 1:]:
                overlap_y = a[0] < b[1] - 0.02 and b[0] < a[1] - 0.02
                overlap_x = a[2] < b[3] - 0.02 and b[2] < a[3] - 0.02
                if overlap_y and overlap_x:
                    problems.append((n, "collides with %r" % b[4], a[4]))
    return problems


# --------------------------------------------------------------------------
# Text metrics -- measured against the real font files.
# --------------------------------------------------------------------------
FONT_FILES = {
    (SANS, 0, 0): "segoeui.ttf", (SANS, 1, 0): "segoeuib.ttf",
    (SANS, 0, 1): "segoeuii.ttf", (SANS, 1, 1): "segoeuiz.ttf",
    (SANS_SB, 0, 0): "seguisb.ttf", (SANS_SB, 1, 0): "seguisb.ttf",
    (SANS_SB, 0, 1): "seguisbi.ttf", (SANS_SB, 1, 1): "seguisbi.ttf",
    (MONO, 0, 0): "consola.ttf", (MONO, 1, 0): "consolab.ttf",
    (SERIF, 0, 0): "pala.ttf", (SERIF, 1, 0): "palab.ttf",
    (SERIF, 0, 1): "palai.ttf", (SERIF, 1, 1): "palabi.ttf",
}
MEASURE_PX = 200
_fcache = {}


def _font(name, bold, italic):
    fn = (FONT_FILES.get((name, int(bool(bold)), int(bool(italic))))
          or FONT_FILES.get((name, 0, 0)) or "segoeui.ttf")
    if fn not in _fcache:
        _fcache[fn] = ImageFont.truetype(os.path.join(FONT_DIR, fn), MEASURE_PX)
    return _fcache[fn]


def measure(text, size_pt, font=SANS, bold=False, italic=False):
    """Rendered width of a string, in inches."""
    return _font(font, bold, italic).getlength(text) / MEASURE_PX * size_pt / 72.0


def wrap(text, width_in, size_pt, font=SANS, bold=False, italic=False):
    """Break into lines the way PowerPoint will -- see FIT."""
    limit = width_in * FIT
    out, cur = [], ""
    for word in text.split():
        cand = word if not cur else cur + " " + word
        if measure(cand, size_pt, font, bold, italic) <= limit:
            cur = cand
        else:
            if cur:
                out.append(cur)
            cur = word
    if cur:
        out.append(cur)
    return out or [""]


def fits_one_line(text, width_in, size_pt, font=SANS, bold=False, italic=False):
    return measure(text, size_pt, font, bold, italic) <= width_in * FIT


def fit_mono(lines, width_in, max_pt=T_CMD_MAX, min_pt=T_CMD_MIN):
    """Largest size at which the longest command line still fits."""
    for size in range(max_pt, min_pt - 1, -1):
        if all(fits_one_line(s, width_in, size, MONO) for s in lines):
            return size
    return min_pt


def fit_sans(text, width_in, max_pt=T_TITLE, min_pt=T_TITLE_MIN, font=SANS_SB,
             bold=False):
    """Largest size at which a headline still holds one line.

    Headlines read as one confident line; a stray two-word second line is the
    thing that makes a slide look broken. Step down rather than wrap. If even
    min_pt will not hold it, return min_pt and let it wrap honestly -- paras_h
    then measures two lines and everything below flows down correctly.
    """
    for size in range(max_pt, min_pt - 1, -1):
        if fits_one_line(text, width_in, size, font, bold):
            return size
    return min_pt


# --------------------------------------------------------------------------
# Primitives
# --------------------------------------------------------------------------
def rect(slide, x, y, w, h, fill=None, line=None, shape=MSO_SHAPE.RECTANGLE,
         line_w=1.0, adj=None):
    s = slide.shapes.add_shape(shape, Inches(x), Inches(y), Inches(w), Inches(h))
    s.shadow.inherit = False
    if adj is not None:
        s.adjustments[0] = adj
    if fill is None:
        s.fill.background()
    else:
        s.fill.solid()
        s.fill.fore_color.rgb = fill
    if line is None:
        s.line.fill.background()
    else:
        s.line.color.rgb = line
        s.line.width = Pt(line_w)
    return s


def para(text, size=T_BODY, font=SANS, bold=False, italic=False, color=INK,
         line=1.22, space=0.0, align=PP_ALIGN.LEFT):
    """One paragraph spec. `space` is space-before, in inches."""
    return dict(text=text, size=size, font=font, bold=bold, italic=italic,
                color=color, line=line, space=space, align=align)


def paras_h(specs, w):
    total = 0.0
    for p in specs:
        n = len(wrap(p["text"], w, p["size"], p["font"], p["bold"], p["italic"]))
        total += p["space"] + n * p["size"] * p["line"] * PP_LINE / 72.0
    return total


def block(slide, x, y, w, specs, anchor=MSO_ANCHOR.TOP, label=""):
    """Lay out paragraphs in one textbox. Returns the exact bottom edge."""
    if isinstance(specs, dict):
        specs = [specs]
    h = paras_h(specs, w)
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h + SLACK))
    tf = box.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = anchor
    for i, p in enumerate(specs):
        pp = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        pp.alignment = p["align"]
        pp.space_before = Pt(p["space"] * 72)
        pp.space_after = Pt(0)
        pp.line_spacing = p["line"]
        run = pp.add_run()
        run.text = p["text"]
        f = run.font
        f.name = p["font"]
        f.size = Pt(p["size"])
        f.bold = p["bold"]
        f.italic = p["italic"]
        f.color.rgb = p["color"]
    return guard(y + h, label) if label else y + h


# --------------------------------------------------------------------------
# Composites
# --------------------------------------------------------------------------
def chip(slide, x, y, label, fill, color=WHITE, size=T_FOOT, pad=0.22, h=0.34):
    w = measure(label, size, SANS, bold=True) + 2 * pad
    rect(slide, x, y, w, h, fill=fill, shape=MSO_SHAPE.ROUNDED_RECTANGLE, adj=0.5)
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    tf = box.text_frame
    tf.word_wrap = False
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.alignment = PP_ALIGN.CENTER
    r = p.add_run()
    r.text = label
    r.font.name = SANS
    r.font.size = Pt(size)
    r.font.bold = True
    r.font.color.rgb = color
    return w


def new_slide(phase, when, title=None, title_size=T_TITLE, deadline="DEMOS 8:30 PM"):
    slide = prs.slides.add_slide(BLANK)
    slide.background.fill.solid()
    slide.background.fill.fore_color.rgb = PAPER

    cw = chip(slide, MARGIN, 0.50, phase, PHASE_COLOR[phase],
              PHASE_TEXT.get(phase, WHITE))
    if when:
        b = slide.shapes.add_textbox(Inches(MARGIN + cw + 0.24), Inches(0.50),
                                     Inches(5.0), Inches(0.34))
        tf = b.text_frame
        tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
        tf.vertical_anchor = MSO_ANCHOR.MIDDLE
        r = tf.paragraphs[0].add_run()
        r.text = when
        r.font.name = SANS
        r.font.size = Pt(T_FOOT + 1)
        r.font.bold = True
        r.font.color.rgb = MUTED

    if title:
        block(slide, MARGIN, 1.00, CW,
              para(title, size=fit_sans(title, CW, max_pt=title_size),
                   font=SANS_SB, line=1.02))

    rect(slide, MARGIN, FOOT_RULE, CW, 0.014, fill=RULE)
    block(slide, MARGIN, FOOT_Y, 6.0,
          para("FAITHTECH TORONTO", size=T_FOOT, color=MUTED, bold=True))
    if deadline:
        block(slide, SW - MARGIN - 6.0, FOOT_Y, 6.0,
              para(deadline, size=T_FOOT, color=MUTED, bold=True,
                   align=PP_ALIGN.RIGHT))
    return slide


def headline(slide, x, y, w, text, size, min_pt=T_TITLE_MIN, line=1.12,
             color=INK, font=SANS_SB, label=""):
    """A statement line, stepped down until it holds a single line."""
    size = fit_sans(text, w, max_pt=size, min_pt=min_pt, font=font)
    return block(slide, x, y, w,
                 para(text, size=size, font=font, line=line, color=color),
                 label=label)


def notes(slide, text):
    slide.notes_slide.notes_text_frame.text = text.strip()


CARD_PAD = 0.22


def _command_specs(w, lines, max_pt=T_CMD_MAX):
    lines = [(ln, INK) if isinstance(ln, str) else ln for ln in lines]
    inner = w - 2 * CARD_PAD - 0.16
    size = fit_mono([t for t, _ in lines], inner, max_pt=max_pt)
    return inner, [para(t, size=size, font=MONO, color=c, line=1.30)
                   for t, c in lines]


def command_h(w, lines, max_pt=T_CMD_MAX):
    inner, specs = _command_specs(w, lines, max_pt)
    return paras_h(specs, inner) + 2 * CARD_PAD


def command_box(slide, x, y, w, lines, max_pt=T_CMD_MAX, spine=LIME, label=""):
    """`lines` are strings, or (text, color) for comment lines."""
    inner, specs = _command_specs(w, lines, max_pt)
    h = paras_h(specs, inner) + 2 * CARD_PAD
    rect(slide, x, y, w, h, fill=CARD, line=RULE)
    rect(slide, x, y, 0.085, h, fill=spine)
    block(slide, x + CARD_PAD + 0.10, y + CARD_PAD, w - 2 * CARD_PAD, specs)
    return guard(y + h, label) if label else y + h


def _callout_spec(w, text, size, font, bold, italic):
    """A callout that misses one line by a word gets a point smaller instead.

    Reserving a second line the renderer does not use leaves the card visibly
    half empty, which reads as a mistake. Three points is the most this will
    give up before letting the text wrap honestly.
    """
    inner = w - 2 * CARD_PAD - 0.30
    for pt in range(size, max(size - 3, T_FOOT) - 1, -1):
        if fits_one_line(text, inner, pt, font, bold, italic):
            size = pt
            break
    return inner, para(text, size=size, font=font, bold=bold, italic=italic,
                       line=1.24)


def callout_h(w, text, size=22, font=SANS, bold=True, italic=False):
    inner, spec = _callout_spec(w, text, size, font, bold, italic)
    return paras_h([spec], inner) + 2 * CARD_PAD


def callout(slide, x, y, w, text, spine=LIME, size=22, font=SANS, bold=True,
            italic=False, label=""):
    inner, spec = _callout_spec(w, text, size, font, bold, italic)
    h = paras_h([spec], inner) + 2 * CARD_PAD
    rect(slide, x, y, w, h, fill=CARD)
    rect(slide, x, y, 0.085, h, fill=spine)
    block(slide, x + CARD_PAD + 0.10, y + CARD_PAD, inner, spec)
    return guard(y + h, label) if label else y + h


def bullet(slide, x, y, w, text, size=T_BODY + 2, marker=LIME, bold=False,
           font=SANS, gap=0.46):
    rect(slide, x + 0.02, y + size * 0.42 / 72.0, 0.17, 0.17, fill=marker)
    return block(slide, x + gap, y, w - gap,
                 para(text, size=size, font=font, bold=bold, line=1.24))


def numbered(slide, x, y, w, n, text, size=T_BODY + 2, fill=LIME, gap=0.64):
    box = 0.42
    rect(slide, x, y - 0.01, box, box, fill=fill,
         shape=MSO_SHAPE.ROUNDED_RECTANGLE, adj=0.18)
    b = slide.shapes.add_textbox(Inches(x), Inches(y - 0.01), Inches(box),
                                 Inches(box))
    tf = b.text_frame
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.alignment = PP_ALIGN.CENTER
    r = p.add_run()
    r.text = str(n)
    r.font.name = SANS
    r.font.size = Pt(T_BODY)
    r.font.bold = True
    r.font.color.rgb = INK
    bot = block(slide, x + gap, y, w - gap, para(text, size=size, line=1.24))
    return max(bot, y + box)


def cards(slide, x, y, w, specs, accent=LIME, pad=0.20, gap=0.16, label=""):
    """Equal-width cards sized to the tallest measured content."""
    n = len(specs)
    cell = (w - (n - 1) * gap) / n
    inner = cell - 2 * pad
    h = max(paras_h(ps, inner) for ps in specs) + 2 * pad + 0.09
    cx = x
    for ps in specs:
        rect(slide, cx, y, cell, h, fill=CARD)
        rect(slide, cx, y, cell, 0.09, fill=accent)
        block(slide, cx + pad, y + 0.09 + pad, inner, ps)
        cx += cell + gap
    return guard(y + h, label) if label else y + h


def wordmark(slide, x, y, w, size=26):
    """Real logo when one has been dropped in, typographic lockup otherwise."""
    if os.path.exists(LOGO):
        slide.shapes.add_picture(LOGO, Inches(x), Inches(y), height=Inches(0.62))
        return
    h = size * 1.22 * PP_LINE / 72.0
    b = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h + 0.02))
    tf = b.text_frame
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    p = tf.paragraphs[0]
    p.line_spacing = 1.22
    for txt, col, fnt in (("FAITHTECH ", INK, SANS_SB), ("TORONTO", MUTED, SANS)):
        r = p.add_run()
        r.text = txt
        r.font.name = fnt
        r.font.size = Pt(size)
        r.font.bold = fnt is SANS_SB
        r.font.color.rgb = col


def agent_run(n, when, title, cmd_lines, output, footnote=None, tip=None,
              tip_spine=BLUE, phase="DEVELOP"):
    """The four kickoffs. A lime band makes them read as events, not slides."""
    slide = new_slide(phase, when)
    rect(slide, 0, 0.92, SW, 1.00, fill=LIME)
    b = slide.shapes.add_textbox(Inches(MARGIN), Inches(0.92), Inches(CW),
                                 Inches(1.00))
    tf = b.text_frame
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    r = tf.paragraphs[0].add_run()
    r.text = "AGENT RUN " + str(n)
    r.font.name = SANS_SB
    r.font.size = Pt(40)
    r.font.bold = True
    r.font.color.rgb = INK

    # The command is the elastic element: a long command plus a long tip can
    # outgrow the slide, so find the largest command size the whole stack
    # tolerates rather than letting the tip fall off the bottom.
    def stack(cmd_pt):
        y = 2.08 + paras_h([para(title, size=T_TITLE, font=SANS_SB, line=1.02)], CW)
        y += 0.18 + command_h(CW, cmd_lines, max_pt=cmd_pt)
        y += 0.15 + paras_h([para(output, size=T_BODY, font=MONO)], CW)
        if footnote:
            y += 0.10 + paras_h([para(footnote, size=T_BODY)], CW)
        if tip:
            y += 0.14 + callout_h(CW, tip, size=T_BODY)
        return y

    cmd_pt = T_CMD_MAX
    while cmd_pt > T_CMD_MIN and stack(cmd_pt) > CONTENT_BOTTOM:
        cmd_pt -= 1

    y = block(slide, MARGIN, 2.08, CW, para(title, size=T_TITLE, font=SANS_SB,
                                            line=1.02))
    y = command_box(slide, MARGIN, y + 0.18, CW, cmd_lines, max_pt=cmd_pt)
    y = block(slide, MARGIN, y + 0.15, CW,
              para(output, size=T_BODY, font=MONO, color=MUTED))
    if footnote:
        y = block(slide, MARGIN, y + 0.10, CW,
                  para(footnote, size=T_BODY, color=MUTED))
    if tip:
        y = callout(slide, MARGIN, y + 0.14, CW, tip, spine=tip_spine, size=T_BODY)
    guard(y, "agent run %d" % n)
    return slide


# ==========================================================================
# GATHER
# ==========================================================================
s = prs.slides.add_slide(BLANK)
s.background.fill.solid()
s.background.fill.fore_color.rgb = PAPER
rect(s, 0, 0, SW, 0.30, fill=LIME)
wordmark(s, MARGIN, 0.95, 6.0, size=24)
block(s, MARGIN, 1.70, CW, para("AI Build Night", size=84, font=SANS_SB, line=1.0))
rect(s, MARGIN, 3.42, 2.60, 0.10, fill=LIME)
block(s, MARGIN, 3.82, CW, [
    para("Wednesday 9 September 2026   ·   5:00 – 9:00 PM", size=26),
    para("Stone Church   ·   45 Davenport Rd, Toronto", size=26, color=MUTED,
         space=0.10)])
x = MARGIN
for label in ("GRAB A NAME TAG", "NAME IN THE RAFFLE BOWL", "PIZZA AT 6:30",
              "SNACKS AND DRINKS ARE OUT"):
    x += chip(s, x, 5.55, label, INK, WHITE, size=13, h=0.42) + 0.16
block(s, MARGIN, FOOT_Y, 6.0,
      para("FAITHTECH TORONTO", size=T_FOOT, color=MUTED, bold=True))
block(s, SW - MARGIN - 6.0, FOOT_Y, 6.0,
      para("A JESUS REVIVAL IN AND THROUGH TECH", size=T_FOOT, color=MUTED,
           bold=True, align=PP_ALIGN.RIGHT))
notes(s, """
5:00-5:20 - DOORS. Leave this slide up while people arrive.
Becky at the door, Jordan on photos and name tags.
Quinntyne floats and starts learning who wants to build what -- this is quiet
Discover work, and it makes the 6:05 team-forming much faster.
Snacks and drinks out from 5:00 (cooler). Raffle bowl by the door -- names in
now, drawn at 8:50.
Anyone who came at 5:00 for setup help: pair them with Javier immediately.
""")

s = new_slide("GATHER", "5:20 PM  ·  8 MIN", "Welcome")
block(s, MARGIN, BODY_TOP, CW - 0.7, para(
    "“You showed up on a Wednesday night with your laptop to build something "
    "for the Kingdom. That’s admirable, it’s appreciated, and it’s seen. "
    "Thank you.”", size=30, font=SERIF, italic=True, line=1.30), label="welcome")
x = MARGIN
for label in ("WASHROOMS", "WIFI", "SNACKS TABLE", "PIZZA 6:30", "RAFFLE 8:50"):
    x += chip(s, x, 5.20, label, INK, WHITE, size=14, h=0.46) + 0.18
block(s, MARGIN, 5.92, CW,
      para("Put your name in the bowl now. We draw at 8:50.", color=MUTED))
notes(s, """
Quinntyne leads. 8 minutes, and hold it -- every minute here comes out of build
time. Say the quote in your own words; do not read it off the screen.
Then logistics: washrooms, wifi password (on the seat-drop card), snacks table,
pizza at 6:30, raffle names in the bowl now and drawn at 8:50.
Thank Stone Church by name here as well as at the close.
""")

s = new_slide("GATHER", "5:20 PM", "Opening prayer")
block(s, MARGIN, BODY_TOP + 0.30, CW - 1.3, para(
    "“Let the favour of the Lord our God be upon us, and establish the work of "
    "our hands upon us — yes, establish the work of our hands.”",
    size=34, font=SERIF, italic=True, line=1.32), label="prayer")
rect(s, MARGIN, 5.20, 1.80, 0.08, fill=LIME)
block(s, MARGIN, 5.50, CW, para("Psalm 90:17", color=MUTED, bold=True))
notes(s, """
Quinntyne leads. Prayer bookends the night -- opening 5:20, closing 8:55.
Pray over the work and over the people it is for, not just over the evening.
Keep it short and unhurried. Nobody is timing this one.
""")

s = new_slide("GATHER", "5:22 PM", "Tonight")
y = headline(s, MARGIN, BODY_TOP, CW - 0.8,
             "By 8:30 you will have a demo video you can share.", 34)
y = cards(s, MARGIN, y + 0.30, CW, [
    [para(w, size=28, font=MONO, bold=True), para(t, color=MUTED, space=0.06)]
    for w, t in (("6:40", "Specs"), ("7:20", "Mocks + design"),
                 ("7:45", "Build"), ("8:10", "Demo video"))], gap=0.20)
block(s, MARGIN, y + 0.26, CW, para(
    "Four agent runs carry the whole night, and each is paired with something "
    "worth doing while it works — eating, teaching, praying, meeting each "
    "other.", color=MUTED, line=1.28), label="tonight")
notes(s, """
This is the shape of the night, and it is the most useful thing to land early.
Once people understand the agent runs are the spine, they stop treating the
breaks as filler and stop watching progress bars.
Becky calls every agent kickoff out loud: 6:40, 7:20, 7:45, 8:10.
Say it plainly: the 8:30 demo start is a hard stop, not a target.
""")

s = new_slide("DISCOVER", "5:28 PM  ·  12 MIN", "Introductions")
y = block(s, MARGIN, BODY_TOP + 0.05, CW,
          para("40 seconds each", size=46, font=SANS_SB))
y += 0.32
for line in ("Your name", "What you make", "What’s on your heart"):
    y = bullet(s, MARGIN, y, CW, line, size=26) + 0.26
block(s, MARGIN, y + 0.18, CW, para("17 of us. We’re keeping time.", color=MUTED),
      label="introductions")
notes(s, """
12 minutes for ~17 people. Becky times, and is allowed to be rude about it.
Hold the line on 40 seconds. If this becomes 25 minutes the build loses a whole
agent run -- that is the actual cost, so say so if it starts to slip.
Model it: go first, and take exactly 40 seconds.
""")

# ==========================================================================
# DISCOVER
# ==========================================================================
s = new_slide("DISCOVER", "5:40 PM  ·  10 MIN", "The 4D cycle")
y = BODY_TOP + 0.06
for word, tag, desc in (
        ("Discover", "See clearly",
         "Reorient through the lens of Christ. Lament. See who it touches most."),
        ("Discern", "Choose wisely",
         "Seek God’s wisdom, then decide: Reject, Receive, Reimagine, or Create."),
        ("Develop", "Co-create",
         "Build in sprints, running the 5R loop on every work item."),
        ("Demonstrate", "Measure love",
         "Redefine impact as friendship compounded by time, not speed shipped.")):
    rect(s, MARGIN, y, 0.52, 0.52, fill=LIME)
    b = s.shapes.add_textbox(Inches(MARGIN), Inches(y), Inches(0.52), Inches(0.52))
    tf = b.text_frame
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    tf.vertical_anchor = MSO_ANCHOR.MIDDLE
    p = tf.paragraphs[0]
    p.alignment = PP_ALIGN.CENTER
    r = p.add_run()
    r.text = "D"
    r.font.name = SANS_SB
    r.font.size = Pt(26)
    r.font.bold = True
    r.font.color.rgb = INK
    head_h = 26 * 1.15 * PP_LINE / 72.0
    hb = s.shapes.add_textbox(Inches(MARGIN + 0.78), Inches(y - 0.03),
                              Inches(CW - 0.78), Inches(head_h + SLACK))
    tf = hb.text_frame
    tf.word_wrap = True
    tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
    p = tf.paragraphs[0]
    p.line_spacing = 1.15
    r = p.add_run()
    r.text = word
    r.font.name = SANS_SB
    r.font.size = Pt(26)
    r.font.bold = True
    r.font.color.rgb = INK
    r2 = p.add_run()
    r2.text = "   " + tag
    r2.font.name = SANS
    r2.font.size = Pt(T_BODY)
    r2.font.color.rgb = MUTED
    bot = block(s, MARGIN + 0.78, y - 0.03 + head_h + 0.03, CW - 0.90,
                para(desc, line=1.20))
    y = max(bot, y + 0.52) + 0.12
guard(y - 1.00 + 0.48 + T_BODY * 1.20 * PP_LINE / 72.0, "4D cycle")
notes(s, """
10 minutes, Quinntyne. Name where tonight sits in each phase:
Discover 5:28-6:05, Discern 6:05-6:15, Develop 6:15-8:10,
Demonstrate 8:10-8:50.
The video is the artifact, the friendship is the impact.
Do not over-teach this. The five R's slide is the one they will actually use.
""")

s = new_slide("DISCOVER", "5:45 PM", "The five R’s")
y = block(s, MARGIN, BODY_TOP - 0.14, CW, para(
    "The loop inside every build block. Not done until all five have happened.",
    color=MUTED))
y = cards(s, MARGIN, y + 0.18, CW, [
    [para(str(i), size=T_BODY, font=MONO, color=MUTED, bold=True),
     para(w, size=24, font=SANS_SB, bold=True, space=0.04),
     para(pn, size=T_POETIC, font=SANS_SB, space=0.07, line=1.15),
     para(d, color=MUTED, space=0.09, line=1.18)]
    for i, (w, pn, d) in enumerate((
        ("Request", "Invite the Spirit into the work",
         "A short, honest ask."),
        ("Receive", "Wait, and write down what comes",
         "Capture it, unedited."),
        ("Review", "Synthesize what emerged",
         "Shape it into a direction."),
        ("Render", "Build toward what you saw",
         "Make it real. Log it."),
        ("Rejoice", "Give thanks for what was made",
         "Name the good it serves.")), start=1)], pad=0.16, gap=0.12)
callout(s, MARGIN, y + 0.14, CW,
        "Rejoice is the one you will skip. Protect the 8:30 demos.",
        spine=LIME, label="five Rs")
notes(s, """
Quinntyne. This is the loop teams run all night, so spend your minutes here
rather than on the 4D slide.
Frame Rejoice deliberately: with agents doing the typing, teams sprint through
Request and Render and skip Receive and Rejoice entirely. That is the failure
mode of an AI build night, and it is why the demos are on the schedule.
""")

s = new_slide("DISCOVER", "5:50 PM  ·  4 MIN", "Project one — RTR")
y = block(s, MARGIN, BODY_TOP - 0.20, CW,
          para("Reconciliation Through Relationships", size=26, font=SANS_SB,
               color=MUTED))
y = block(s, MARGIN, y + 0.18, CW, para(
    "Built at July’s hackathon with rightrelationship.ca, in response to the "
    "TRC’s Calls to Action.", line=1.26))
y += 0.20
for line in ("Registration, onboarding, and a shared learning journey",
             "Facilitator-reviewed matching, cohort map, messaging, scheduling"):
    y = bullet(s, MARGIN, y, CW, line) + 0.16
h = 0.48
rect(s, MARGIN, y + 0.06, CW, h, fill=CARD)
b = s.shapes.add_textbox(Inches(MARGIN + 0.26), Inches(y + 0.06), Inches(CW - 0.5),
                         Inches(h))
tf = b.text_frame
tf.margin_left = tf.margin_right = tf.margin_top = tf.margin_bottom = 0
tf.vertical_anchor = MSO_ANCHOR.MIDDLE
r = tf.paragraphs[0].add_run()
r.text = "Next.js 16   ·   React 19   ·   Tailwind 4   ·   Supabase"
r.font.name = MONO
r.font.size = Pt(T_BODY)
r.font.color.rgb = INK
callout(s, MARGIN, y + h + 0.20, CW,
        "Runs on mock data — clone it and you are building in five minutes.",
        spine=LIME, size=T_BODY, label="RTR")
notes(s, """
Quinntyne, 4 minutes. Keep it to what someone would need in order to decide to
join. This is the safe landing spot for anyone who arrives without an idea, and
for solo builders if turnout is light -- fold them in here rather than starting
something cold.
Have the repo URL ready to paste into Slack the moment you finish.
""")

s = new_slide("DISCOVER", "5:55 PM  ·  10 MIN", "Project two")
block(s, MARGIN, BODY_TOP + 0.30, CW, para("Over to you", size=58, font=SANS_SB))
rect(s, MARGIN, 4.15, 2.60, 0.10, fill=LIME)
block(s, MARGIN, 4.55, CW - 0.8, para(
    "Then the invitation, to everyone: who wants in on either project? "
    "Teams of two or three.", size=26, line=1.26), label="project two")
notes(s, """
Guest presenter, 10 minutes, confirmed Tuesday. Hand over cleanly and get out of
the way -- then hold him to 10 minutes.
Ask him to end by naming the help he actually wants, not just the idea.
Then open the invitation to the room: RTR, this project, or your own.
CONTINGENCY: if the pitch runs long, cut the 7:20 networking break to 10 minutes.
Never cut dinner and never cut the demos.
""")

# ==========================================================================
# DISCERN
# ==========================================================================
s = new_slide("DISCERN", "6:05 PM  ·  10 MIN", "Lament, then discern")
y = block(s, MARGIN, BODY_TOP - 0.10, CW, para(
    "Name what is broken, and who it hurts — before you name a feature.",
    size=26, line=1.24))
y = cards(s, MARGIN, y + 0.28, CW, [
    [para(w, size=24, font=SANS_SB, bold=True),
     para(g, color=MUTED, space=0.08, line=1.18)]
    for w, g in (("Reject", "Should not exist"),
                 ("Receive", "Use it as it is"),
                 ("Reimagine", "Redeem what is"),
                 ("Create", "Make it new"))], accent=GREEN, gap=0.18)
callout(s, MARGIN, y + 0.24, CW,
        "Most ideas tonight are Reimagine or Receive — not Create.",
        spine=GREEN, label="lament")
notes(s, """
Quinntyne leads, Javier works the room. 10 minutes.
Run the discernment call on each idea OUT LOUD -- it teaches the framework far
better than the slide does, and it kills the unbuildable ideas kindly.
Lament first. Do not let anyone jump to a feature list before they have named
who the problem hurts.
Javier: start forming teams of 2-3 while this is happening.
""")

s = new_slide("DISCERN", "6:10 PM", "Scope down")
y = block(s, MARGIN, BODY_TOP + 0.05, CW - 0.9, para(
    "“If it can’t be shown in a 90-second video by 8:30, it’s too big. "
    "Cut it again.”", size=40, font=SERIF, italic=True, line=1.24))
y += 0.50
for line in ("Teams of two or three.",
             "Write your build in one sentence on the table tent."):
    y = bullet(s, MARGIN, y, CW, line, size=26) + 0.36
guard(y, "scope down")
notes(s, """
This is the highest-leverage sentence of the night. Say it twice.
Javier and Quinntyne go table to table and cut scope out loud -- teams will not
cut hard enough on their own, and a friendly outside voice makes it painless.
One sentence on the table tent, visible all night. If a team cannot write the
sentence, the scope is still too big.
Anyone still solo at 6:25 gets paired with a team that is already moving.
""")

# ==========================================================================
# DEVELOP
# ==========================================================================
s = new_slide("DEVELOP", "6:15 PM  ·  15 MIN", "Setup sprint")
y = BODY_TOP - 0.02
for i, line in enumerate((
        "Create the GitHub repo — prove push access now",
        "Install the MCP servers your project needs",
        "Confirm the agent-toolkit plugin loaded",
        "Generate your agent instruction files"), start=1):
    y = numbered(s, MARGIN, y, CW, i, line) + 0.14
command_box(s, MARGIN, y + 0.18, CW, [
    "/agent-toolkit:agent-instruction-files <your project>",
    ("# it will ask: web or cli — know your answer", ORANGE)],
    label="setup sprint")
notes(s, """
15 minutes, everyone at once. Javier and Quinntyne work the room.
Produces AGENTS.md plus CLAUDE.md, GEMINI.md and copilot-instructions.

If someone did not do the pre-flight install:
  claude plugin marketplace add QuinntyneBrown/agent-toolkit
  claude plugin install agent-toolkit@agent-toolkit

The silent killers are Python 3.10+ on PATH, and Java + PlantUML. A team
discovers the missing PlantUML at 7:20 when the diagrams will not render.
Check both now, not later.

Anyone still stuck at 6:25 pairs up with a team that is not. Say out loud now
that a team which finishes early goes and helps a stuck team -- said in advance
it feels expected rather than like charity.
""")

s = new_slide("DEVELOP", "6:30 PM  ·  10 MIN", "Write the mini-PRD")
y = block(s, MARGIN, BODY_TOP - 0.20, CW, para(
    "One page of prose. Ten minutes. This is the seed for everything downstream.",
    size=T_BODY + 2, color=MUTED))
y += 0.28
for line in ("Who it is for", "The problem", "What “working” looks like"):
    y = bullet(s, MARGIN, y, CW, line, size=28) + 0.24
callout(s, MARGIN, y + 0.20, CW,
        "Hard stop at 6:40. It goes to the agent whether it is pretty or not.",
        spine=ORANGE, size=24, label="mini-PRD")
notes(s, """
10 minutes. Teams write; do not let them start coding.
Prose, not bullet points -- the requirements skill does better with prose.
Hold 6:40 absolutely. An ugly PRD that reaches the agent at 6:40 beats a polished
one at 6:55, because dinner is the agent's window and the window does not move.
Becky calls the 6:40 kickoff.
""")

agent_run(1, "6:40 PM", "Build the specs",
          ["/agent-toolkit:requirements-engineer",
           "  <paste your mini-PRD>"],
          "→ docs/specs/L1.md + L2.md",
          "L1 capabilities, L2 behaviours with Given/When/Then. Every L2 "
          "traces to an L1.")
notes(prs.slides[-1], """
Becky calls this out loud. Every team starts the run before they touch food.
The agent writes L1 and L2 through the whole dinner block -- that is the design.
Do not let anyone sit and watch it. Get them to the pizza.
""")

s = new_slide("DEVELOP", "6:40 PM  ·  30 MIN", "Pizza, and four ideas")
y = block(s, MARGIN, BODY_TOP - 0.22, CW,
          para("Eat properly. This is the one real break of the night.",
               size=T_BODY + 2, color=MUTED))
y = cards(s, MARGIN, y + 0.26, CW, [
    [para(w, size=24, font=SANS_SB, bold=True, line=1.10),
     para(d, color=MUTED, space=0.10, line=1.18)]
    for w, d in (
        ("MCP", "How your agent reaches tools it did not ship with."),
        ("AGENTS.md", "Durable memory. Written once, read every run."),
        ("Skills", "A reusable procedure you hand your agent."),
        ("Agentic workflows", "Agents that run on the repo, not your laptop."))],
    accent=ORANGE, gap=0.18)
callout(s, MARGIN, y + 0.26, CW,
        "Your agents are writing L1 and L2 this entire time.", spine=LIME,
        label="fireside")
notes(s, """
~12 minutes standing up, over the food. No slides beyond this one -- talk.
Becky on food; pizza was ordered at 5:45 for 6:30 delivery.
Then take questions until 7:10. Do not fill the whole block with teaching --
people need to eat and the room needs to breathe.
Budget: $65 across ~17 people is about $3.80 a head. A top-up of $30-40 is
expected -- keep the receipt for FaithTech.
""")

s = new_slide("DEVELOP", "7:10 PM  ·  10 MIN", "Read what the agent wrote")
y = block(s, MARGIN, BODY_TOP - 0.06, CW,
          para("Receive → Review", size=40, font=SANS_SB))
y = block(s, MARGIN, y + 0.28, CW, para(
    "Wrong requirements compound into wrong code faster than you can catch them.",
    size=T_BODY + 2, line=1.26))
y = block(s, MARGIN, y + 0.24, CW, para(
    "Then write your mock prompt — and say this in it, explicitly:",
    size=T_BODY + 2, bold=True))
callout(s, MARGIN, y + 0.20, CW,
        "HTML mocks are design artifacts, not production code. No ATDD, no "
        "tests, throwaway.", spine=BLUE, size=24, label="review specs")
notes(s, """
10 minutes, teams. This is the Review step of the five R's, and it is the one
teams skip when the agent is fast.
Make them actually open L1.md and L2.md and read them. Ask one team to read a
requirement out loud -- it usually surfaces a wrong assumption immediately.
Then the mock prompt, with the design-artifact line in it. Leave that line out
and the agent will try to build the mocks properly and burn the window.
Becky calls the 7:20 kickoff.
""")

agent_run(2, "7:20 PM", "Mocks and detailed design",
          ["<your mock prompt>",
           "/agent-toolkit:software-design-document",
           "  Create detailed designs from docs/specs/."],
          "→ docs/detailed-designs/  ·  HTML mocks",
          "C4, class and sequence diagrams rendered to PNG. Needs Java and "
          "PlantUML on PATH.")
notes(prs.slides[-1], """
Becky calls it. Both things start now -- the mocks and the design documents.
The design skill REFUSES to run without L1 and L2 already in docs/specs/. It
stops and asks rather than inventing them, so a team that skipped the 6:40 run
gets caught here. Watch for it.
This is also where a missing Java/PlantUML install shows up. Fallback: they carry
on and demo the mocks plus the specs.
""")

s = new_slide("DEVELOP", "7:20 PM  ·  15 MIN", "Stand up")
y = block(s, MARGIN, BODY_TOP + 0.15, CW, para("15 minutes", size=58,
                                               font=SANS_SB))
y += 0.44
for line in ("Refill. Change tables.",
             "Introduce two people who have not spoken yet."):
    y = bullet(s, MARGIN, y, CW, line, size=26) + 0.36
block(s, MARGIN, y + 0.24, CW,
      para("Your agent is working. Let it.", size=T_BODY + 2, color=MUTED,
           italic=True), label="break")
notes(s, """
A real break, and it is deliberate -- friendship is the point, not just the build.
Jordan: photos, and 2-3 short testimonials on camera while the room is warm.
This is the block to cut to 10 minutes if the guest pitch ran long.
Quinntyne and Javier: use it to check on the quietest team, not the loudest.
""")

s = new_slide("DEVELOP", "7:35 PM  ·  10 MIN", "Resolve the drift")
y = headline(s, MARGIN, BODY_TOP - 0.04, CW - 0.6,
             "Your specs, designs and mocks now disagree. They always do.",
             28, line=1.24, font=SANS)
y = block(s, MARGIN, y + 0.34, CW, para(
    "Ask your agent to reconcile all three — and to report every contradiction "
    "it found.", size=T_BODY + 2, line=1.26))
callout(s, MARGIN, y + 0.32, CW,
        "Do this before a single line of production code exists. It is far "
        "cheaper here than at 8:15.", spine=ORANGE, size=24, label="drift")
notes(s, """
10 minutes, teams. Easy to skip, expensive to skip.
Make them ask for the LIST of contradictions, not just a silent fix -- the list
is what teaches them where the drift comes from.
Becky calls the 7:45 kickoff.
""")

agent_run(3, "7:45 PM", "Build it — incrementally",
          ["failing acceptance test  →  code  →  green  →  commit"],
          "→ ATDD + Addy Osmani’s incremental implementation skill",
          "One vertical slice at a time. No slice without an L2 behind it.",
          tip="8:00 — thirty minutes left. Stop adding. Start cutting.",
          tip_spine=ORANGE)
notes(prs.slides[-1], """
25 minutes, the quietest stretch of the night.
Quinntyne and Javier float constantly. The failure mode here is a team silently
stuck for fifteen minutes -- keep circling, and ask "what is on screen right
now?" rather than "how is it going?".
Becky calls the 8:00 thirty-minute warning OUT LOUD. Any team without something
on screen by then stops adding and starts cutting.
""")

# ==========================================================================
# DEMONSTRATE
# ==========================================================================
agent_run(4, "8:10 PM", "Record the demo",
          ["/agent-toolkit:demo-video",
           "  Create a demo video for each executable application."],
          "→ docs/demo/  ·  silent continuous take, 1280×720",
          tip="No running code? Run it against your mocks. Everyone still "
              "leaves with a video.",
          tip_spine=BLUE, phase="DEMONSTRATE")
notes(prs.slides[-1], """
Becky calls it. Every team starts the render now, then writes their words while
it runs.
Javier collects the video files on the presenting laptop by 8:25. Not 8:31.
CONTINGENCY: an agent that stalled or ate its window still demos -- the specs and
the mocks ARE a real demo, and they show the process, which is what tonight is
teaching.
""")

s = new_slide("DEMONSTRATE", "8:10 PM  ·  20 MIN", "Write your 60 seconds")
y = block(s, MARGIN, BODY_TOP - 0.22, CW,
          para("While the video renders. Four lines, one speaker.",
               size=T_BODY + 2, color=MUTED))
y += 0.18
for i, line in enumerate(("The problem", "Who it serves",
                          "What you watched the agent do", "What is next"),
                         start=1):
    y = numbered(s, MARGIN, y, CW, i, line, size=26) + 0.12
callout(s, MARGIN, y + 0.24, CW,
        "Video files to Javier on the presenting laptop by 8:25.",
        spine=ORANGE, size=T_BODY + 2, label="60 seconds")
notes(s, """
20 minutes while the renders run. Teams write; they do not keep building.
"What you watched the agent do" is the interesting line, and the one people
forget -- it is what makes this a build night rather than a demo night.
Javier collects files by 8:25 on the presenting laptop and confirms each one
actually plays.
""")

s = new_slide("DEMONSTRATE", "8:30 PM  ·  HARD START", "Demos")
y = headline(s, MARGIN, BODY_TOP + 0.05, CW,
             "3 minutes each — the video, then 30 seconds of words.", 32,
             line=1.18)
y += 0.32
for line in ("No live coding.", "No apologies for what is not finished."):
    y = bullet(s, MARGIN, y, CW, line, size=26, marker=AMBER) + 0.24
callout(s, MARGIN, y + 0.18, CW,
        "This is Rejoice. It is the fifth R, and it is on the schedule for a "
        "reason.", spine=LIME, size=24, label="demos")
notes(s, """
HARD START at 8:30. Becky holds it. ~5 teams, 3 minutes each, 20 minutes total.
Jordan records the demos.
Do not let a team apologise their way through their slot -- redirect them to what
they learned. Celebrate loudly and specifically; name what each team did that was
good. This block is the room's shared Rejoice, and it sets whether people come
back next time.
""")

s = new_slide("SEND", "8:50 PM  ·  5 MIN", "Raffle", deadline=None)
y = block(s, MARGIN, BODY_TOP + 0.35, CW,
          para("Names out of the bowl.", size=48, font=SANS_SB))
block(s, MARGIN, y + 0.46, CW,
      para("Thank you for coming, and for building.", size=26, color=MUTED),
      label="raffle")
notes(s, """
Becky runs the draw. Jordan gets the photo.
5 minutes -- keep it light and quick. It is a thank-you, not a segment.
""")

s = new_slide("SEND", "8:55 PM  ·  5 MIN", "What’s next", deadline=None)
y = BODY_TOP + 0.15
for line in ("Keep your repo alive past tonight.",
             "The next FaithTech Toronto date.",
             "Thank you, Stone Church."):
    y = bullet(s, MARGIN, y, CW, line, size=28) + 0.38
rect(s, MARGIN, y + 0.22, 1.80, 0.08, fill=LIME)
block(s, MARGIN, y + 0.52, CW,
      para("Closing prayer", size=30, font=SERIF, italic=True), label="what's next")
notes(s, """
Quinntyne. Name the next event date concretely, so people can put it in a
calendar tonight while they still feel it.
Say how to keep the repos alive: who owns each one, where it lives, what the next
commit would be.
Thank Stone Church by name. We are coming back, and the room should be left
better than they handed it to us.
Then close in prayer over the work and the people it is for.
Teardown 9:00-9:20, all hands.
""")

s = prs.slides.add_slide(BLANK)
s.background.fill.solid()
s.background.fill.fore_color.rgb = PAPER
rect(s, 0, 0, SW, 0.30, fill=LIME)
block(s, MARGIN, 2.50, CW - 1.0,
      para("Build well, and rejoice.", size=66, font=SERIF, italic=True, line=1.10))
rect(s, MARGIN, 4.45, 2.60, 0.10, fill=LIME)
block(s, MARGIN, 4.85, CW,
      para("Take your video with you. Share it.", size=26))
wordmark(s, MARGIN, 5.80, 6.0, size=22)
block(s, SW - MARGIN - 6.0, FOOT_Y, 6.0,
      para("9 SEPTEMBER 2026   ·   STONE CHURCH", size=T_FOOT, color=MUTED,
           bold=True, align=PP_ALIGN.RIGHT))
notes(s, """
Leave this up through teardown.
Teardown 9:00-9:20, all hands: cooler out, garbage bagged, chairs and tables
back, lights, lock up.
""")

# --------------------------------------------------------------------------
cp = prs.core_properties
cp.title = "AI Build Night - FaithTech Toronto"
cp.subject = "Participant deck, 9 September 2026, Stone Church"
cp.author = "FaithTech Toronto"
cp.comments = ("Generated by docs/build_deck.py. Edit the script and re-run; "
               "do not hand-edit this file.")

failed = False

if _overflows:
    failed = True
    print("OVERFLOW - content crosses %.2fin:" % CONTENT_BOTTOM)
    for n, label, y in _overflows:
        print("  slide %2d  %-16s bottom=%.3f" % (n, label, y))

problems = audit()
if problems:
    failed = True
    print("AUDIT - %d problem(s):" % len(problems))
    for n, what, label in problems:
        print("  slide %2d  %-34s %s" % (n, what, label))

if failed:
    sys.exit(1)

prs.save(OUT)
print("wrote {}  ({} slides, audit clean)".format(OUT, len(prs.slides)))
